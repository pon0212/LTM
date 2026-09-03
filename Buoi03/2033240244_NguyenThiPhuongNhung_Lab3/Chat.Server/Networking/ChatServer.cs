using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Chat.Shared.Protocol;

namespace Chat.Server.Networking;

public sealed class ChatServer : IDisposable
{
    private readonly ConcurrentDictionary<string, ClientSession> _clients =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly object _stateLock = new();

    private TcpListener? _listener;
    private Thread? _acceptThread;
    private volatile bool _isRunning;

    public event Action<string>? LogReceived;
    public event Action<IReadOnlyList<string>>? ClientListChanged;

    public bool IsRunning => _isRunning;

    public void Start(IPAddress address, int port)
    {
        if (port is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(
                nameof(port),
                "Port phải từ 1 đến 65535.");
        }

        lock (_stateLock)
        {
            if (_isRunning)
            {
                throw new InvalidOperationException(
                    "Server đang chạy.");
            }

            _listener = new TcpListener(address, port);
            _listener.Start();

            _isRunning = true;

            _acceptThread = new Thread(AcceptLoop)
            {
                IsBackground = true,
                Name = "ChatServer-AcceptThread"
            };

            _acceptThread.Start();
        }

        WriteLog($"Server đã chạy tại {address}:{port}.");
    }

    public void Stop()
    {
        Thread? acceptThread;

        lock (_stateLock)
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;
            acceptThread = _acceptThread;

            try
            {
                _listener?.Stop();
            }
            catch
            {
            }
        }

        foreach (KeyValuePair<string, ClientSession> pair
                 in _clients.ToArray())
        {
            if (_clients.TryRemove(
                    pair.Key,
                    out ClientSession? session))
            {
                session.Dispose();
            }
        }

        PublishClientList();

        if (acceptThread is not null &&
            acceptThread != Thread.CurrentThread)
        {
            acceptThread.Join(millisecondsTimeout: 1000);
        }

        WriteLog("Server đã dừng.");
    }

    private void AcceptLoop()
    {
        while (_isRunning)
        {
            try
            {
                TcpClient tcpClient =
                    _listener!.AcceptTcpClient();

                tcpClient.NoDelay = true;

                var clientThread =
                    new Thread(() => HandleClient(tcpClient))
                    {
                        IsBackground = true,
                        Name =
                            $"ChatServer-Client-{tcpClient.Client.RemoteEndPoint}"
                    };

                clientThread.Start();
            }
            catch (SocketException) when (!_isRunning)
            {
                break;
            }
            catch (ObjectDisposedException) when (!_isRunning)
            {
                break;
            }
            catch (Exception ex)
            {
                WriteLog(
                    $"Lỗi nhận kết nối: {ex.Message}");
            }
        }
    }

    private void HandleClient(TcpClient tcpClient)
    {
        ClientSession? session = null;
        bool wasAdded = false;

        try
        {
            session = new ClientSession(tcpClient);

            // Bản tin đầu tiên bắt buộc là Login.
            ChatMessage? login =
                session.ReadMessage();

            string username =
                login?.Sender.Trim() ?? string.Empty;

            if (login?.Type != MessageType.Login ||
                !IsValidUsername(username))
            {
                session.SendMessage(
                    new ChatMessage
                    {
                        Type = MessageType.LoginResult,
                        Success = false,
                        Content =
                            "Tên đăng nhập phải có 1-20 ký tự và không chứa ký tự điều khiển."
                    });

                return;
            }

            session.Username = username;

            // TryAdd là thao tác nguyên tử:
            // hai Client không thể chiếm cùng username.
            if (!_clients.TryAdd(username, session))
            {
                session.SendMessage(
                    new ChatMessage
                    {
                        Type = MessageType.LoginResult,
                        Success = false,
                        Content =
                            "Tên đăng nhập đã được sử dụng."
                    });

                return;
            }

            wasAdded = true;

            session.SendMessage(
                new ChatMessage
                {
                    Type = MessageType.LoginResult,
                    Success = true,
                    Content = "Đăng nhập thành công."
                });

            WriteLog(
                $"{username} đã kết nối từ {tcpClient.Client.RemoteEndPoint}.");

            BroadcastSystem(
                $"{username} đã tham gia phòng chat.");

            BroadcastUserList();

            while (_isRunning)
            {
                ChatMessage? incoming =
                    session.ReadMessage();

                if (incoming is null ||
                    incoming.Type == MessageType.Disconnect)
                {
                    break;
                }

                if (incoming.Type != MessageType.Chat)
                {
                    continue;
                }

                string content =
                    incoming.Content.Trim();

                if (content.Length is < 1 or > 1000)
                {
                    session.SendMessage(
                        new ChatMessage
                        {
                            Type = MessageType.Error,
                            Content =
                                "Tin nhắn phải có từ 1 đến 1000 ký tự."
                        });

                    continue;
                }

                // Server tự gắn Sender và thời gian.
                Broadcast(
                    new ChatMessage
                    {
                        Type = MessageType.Chat,
                        Sender = username,
                        Content = content,
                        SentAt = DateTimeOffset.Now
                    });

                WriteLog(
                    $"{username}: {content}");
            }
        }
        catch (IOException ex)
        {
            if (_isRunning && wasAdded)
            {
                WriteLog(
                    $"Mất kết nối với {session?.Username}: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            if (_isRunning)
            {
                WriteLog(
                    $"Lỗi xử lý Client: {ex.Message}");
            }
        }
        finally
        {
            if (session is not null &&
                wasAdded &&
                _clients.TryRemove(
                    session.Username,
                    out ClientSession? removed))
            {
                removed.Dispose();

                WriteLog(
                    $"{session.Username} đã ngắt kết nối.");

                if (_isRunning)
                {
                    BroadcastSystem(
                        $"{session.Username} đã rời phòng chat.");

                    BroadcastUserList();
                }
                else
                {
                    PublishClientList();
                }
            }
            else
            {
                session?.Dispose();
                tcpClient.Dispose();
            }
        }
    }

    private void BroadcastSystem(string content) =>
        Broadcast(
            new ChatMessage
            {
                Type = MessageType.System,
                Sender = "SERVER",
                Content = content,
                SentAt = DateTimeOffset.Now
            });

    private void BroadcastUserList()
    {
        List<string> users =
            GetSortedUsers();

        Broadcast(
            new ChatMessage
            {
                Type = MessageType.UserList,
                Users = users
            });

        ClientListChanged?.Invoke(users);
    }

    private void Broadcast(ChatMessage message)
    {
        foreach (KeyValuePair<string, ClientSession> pair
                 in _clients.ToArray())
        {
            try
            {
                pair.Value.SendMessage(message);
            }
            catch
            {
                bool removed =
                    _clients.TryRemove(pair);

                if (removed)
                {
                    pair.Value.Dispose();

                    WriteLog(
                        $"Đã loại kết nối hỏng của {pair.Key}.");

                    PublishClientList();
                }
            }
        }
    }

    private void PublishClientList() =>
        ClientListChanged?.Invoke(
            GetSortedUsers());

    private List<string> GetSortedUsers() =>
        _clients.Keys
            .OrderBy(
                name => name,
                StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static bool IsValidUsername(string username) =>
        username.Length is >= 1 and <= 20 &&
        username.All(c => !char.IsControl(c));

    private void WriteLog(string text) =>
        LogReceived?.Invoke(
            $"[{DateTime.Now:HH:mm:ss}] {text}");

    public void Dispose() => Stop();
}