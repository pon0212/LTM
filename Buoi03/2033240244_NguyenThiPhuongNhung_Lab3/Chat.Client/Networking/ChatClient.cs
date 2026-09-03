using System.Net.Sockets;
using System.Text;
using Chat.Shared.Protocol;

namespace Chat.Client.Networking;

public sealed class ChatClient : IDisposable
{
    private readonly object _sendLock = new();

    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private Thread? _receiveThread;

    private volatile bool _isConnected;
    private int _closed = 1;

    public bool IsConnected => _isConnected;

    public event Action<ChatMessage>? MessageReceived;

    public event Action<IReadOnlyList<string>>?
        UserListReceived;

    public event Action<string>? ErrorOccurred;

    public event Action<string>? Disconnected;

    public void Connect(
        string host,
        int port,
        string username)
    {
        if (_isConnected)
        {
            throw new InvalidOperationException(
                "Client đang kết nối.");
        }

        username = username.Trim();

        if (username.Length is < 1 or > 20 ||
            username.Any(char.IsControl))
        {
            throw new ArgumentException(
                "Tên đăng nhập phải có 1-20 ký tự và không chứa ký tự điều khiển.",
                nameof(username));
        }

        var tcpClient = new TcpClient
        {
            NoDelay = true
        };

        try
        {
            tcpClient.Connect(host, port);

            NetworkStream stream =
                tcpClient.GetStream();

            var utf8 =
                new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier: false);

            var reader = new StreamReader(
                stream,
                utf8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 4096,
                leaveOpen: true);

            var writer = new StreamWriter(
                stream,
                utf8,
                bufferSize: 4096,
                leaveOpen: true)
            {
                AutoFlush = true
            };

            // Gửi Login trước khi khởi động receive thread.
            writer.WriteLine(
                ProtocolSerializer.Serializer(
                    new ChatMessage
                    {
                        Type = MessageType.Login,
                        Sender = username
                    }));

            string? loginLine =
                reader.ReadLine();

            if (loginLine is null)
            {
                throw new IOException(
                    "Server đã đóng kết nối khi đăng nhập.");
            }

            ChatMessage loginResult =
                ProtocolSerializer.Deserializer(
                    loginLine);

            if (loginResult.Type !=
                    MessageType.LoginResult ||
                !loginResult.Success)
            {
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(
                        loginResult.Content)
                        ? "Đăng nhập thất bại."
                        : loginResult.Content);
            }

            _tcpClient = tcpClient;
            _stream = stream;
            _reader = reader;
            _writer = writer;

            Interlocked.Exchange(
                ref _closed,
                0);

            _isConnected = true;

            _receiveThread =
                new Thread(ReceiveLoop)
                {
                    IsBackground = true,
                    Name =
                        "ChatClient-ReceiveThread"
                };

            _receiveThread.Start();
        }
        catch
        {
            tcpClient.Dispose();
            throw;
        }
    }

    public void SendChat(string content)
    {
        content = content.Trim();

        if (!_isConnected)
        {
            throw new InvalidOperationException(
                "Chưa kết nối đến Server.");
        }

        if (content.Length is < 1 or > 1000)
        {
            throw new ArgumentException(
                "Tin nhắn phải có từ 1 đến 1000 ký tự.",
                nameof(content));
        }

        SendMessage(
            new ChatMessage
            {
                Type = MessageType.Chat,
                Content = content
            });
    }

    public void Disconnect()
    {
        if (_isConnected)
        {
            try
            {
                SendMessage(
                    new ChatMessage
                    {
                        Type =
                            MessageType.Disconnect
                    });
            }
            catch
            {
            }
        }

        CloseConnection();
    }

    private void ReceiveLoop()
    {
        string reason =
            "Kết nối đến Server đã đóng.";

        try
        {
            while (_isConnected)
            {
                string? line =
                    _reader!.ReadLine();

                if (line is null)
                {
                    break;
                }

                ChatMessage message =
                    ProtocolSerializer.Deserializer(
                        line);

                if (message.Type ==
                    MessageType.UserList)
                {
                    UserListReceived?.Invoke(message.Users ?? new List<string>());
                }
                else
                {
                    MessageReceived?.Invoke(
                        message);
                }
            }
        }
        catch (Exception ex)
        {
            if (_isConnected)
            {
                reason =
                    $"Mất kết nối: {ex.Message}";

                ErrorOccurred?.Invoke(
                    ex.Message);
            }
        }
        finally
        {
            bool wasConnected =
                _isConnected;

            CloseConnection();

            if (wasConnected)
            {
                Disconnected?.Invoke(reason);
            }
        }
    }

    private void SendMessage(
        ChatMessage message)
    {
        string json =
            ProtocolSerializer.Serializer(
                message);

        lock (_sendLock)
        {
            if (!_isConnected ||
                _writer is null)
            {
                throw new InvalidOperationException(
                    "Chưa kết nối đến Server.");
            }

            _writer.WriteLine(json);
        }
    }

    private void CloseConnection()
    {
        if (Interlocked.Exchange(
                ref _closed,
                1) == 1)
        {
            return;
        }

        _isConnected = false;

        try
        {
            _tcpClient?.Client.Shutdown(
                SocketShutdown.Both);
        }
        catch
        {
        }

        try { _reader?.Dispose(); } catch { }
        try { _writer?.Dispose(); } catch { }
        try { _stream?.Dispose(); } catch { }
        try { _tcpClient?.Dispose(); } catch { }

        _reader = null;
        _writer = null;
        _stream = null;
        _tcpClient = null;
    }

    public void Dispose() =>
        Disconnect();
}