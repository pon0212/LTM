using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using FileTransfer.Shared;

namespace FileSocketServer.Services;

public sealed class FileServer
{
    private const int FileBufferSize = 81_920;

    private readonly int _port;
    private readonly int _throttleMilliseconds;
    private readonly string _serverFilesDirectory;
    private readonly TcpListener _listener;

    private readonly ConcurrentDictionary<int, Task> _clientTasks = new();

    private int _nextClientId;

    public FileServer(int port, int throttleMilliseconds)
    {
        if (port is < 1 or > 65_535)
            throw new ArgumentOutOfRangeException(nameof(port));

        if (throttleMilliseconds is < 0 or > 60_000)
            throw new ArgumentOutOfRangeException(nameof(throttleMilliseconds));

        _port = port;
        _throttleMilliseconds = throttleMilliseconds;

        _serverFilesDirectory =
            Path.Combine(AppContext.BaseDirectory, "ServerFiles");

        _listener = new TcpListener(IPAddress.Any, _port);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await PrepareServerFilesAsync(cancellationToken);

        _listener.Start();

        Log($"Server đang lắng nghe tại 0.0.0.0:{_port}");
        Log($"Thư mục chia sẻ: {_serverFilesDirectory}");
        Log($"Throttle: {_throttleMilliseconds} ms/khối");
        Log("Nhấn Ctrl+C để dừng Server an toàn.");

        using CancellationTokenRegistration stopRegistration =
            cancellationToken.Register(() => _listener.Stop());

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client;

                try
                {
                    client =
                        await _listener.AcceptTcpClientAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (SocketException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (ObjectDisposedException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                int clientId =
                    Interlocked.Increment(ref _nextClientId);

                Task clientTask =
                    HandleClientAsync(
                        clientId,
                        client,
                        cancellationToken);

                _clientTasks[clientId] = clientTask;

                _ = clientTask.ContinueWith(
                    completedTask =>
                        _clientTasks.TryRemove(clientId, out _),
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }
        }
        finally
        {
            _listener.Stop();

            Log("Đang chờ các kết nối Client kết thúc...");

            Task[] remainingTasks =
                _clientTasks.Values.ToArray();

            if (remainingTasks.Length > 0)
                await Task.WhenAll(remainingTasks);

            Log("Server đã dừng.");
        }
    }

    private async Task PrepareServerFilesAsync(
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_serverFilesDirectory);

        string welcomePath =
            Path.Combine(
                _serverFilesDirectory,
                "Welcome.txt");

        if (!File.Exists(welcomePath))
        {
            const string content =
                "Chào mừng đến với LAB 6 - Truyền file bằng TCP Socket.\r\n" +
                "File này được Server tự động tạo.\r\n";

            await File.WriteAllTextAsync(
                welcomePath,
                content,
                new UTF8Encoding(false),
                cancellationToken);
        }
    }

    private async Task HandleClientAsync(
        int clientId,
        TcpClient client,
        CancellationToken cancellationToken)
    {
        string remoteEndpoint =
            client.Client.RemoteEndPoint?.ToString()
            ?? "không xác định";

        LogClient(
            clientId,
            $"Đã kết nối từ {remoteEndpoint}");

        using (client)
        {
            client.NoDelay = true;

            try
            {
                using NetworkStream stream =
                    client.GetStream();

                DownloadRequest request =
                    await NetworkProtocol
                        .ReadMessageAsync<DownloadRequest>(
                            stream,
                            cancellationToken);

                LogClient(
                    clientId,
                    $"Yêu cầu file: '{request.FileName}'");

                if (!string.Equals(
                    request.Command,
                    "DOWNLOAD",
                    StringComparison.Ordinal))
                {
                    await SendFailureAsync(
                        stream,
                        request.FileName,
                        "Command không được hỗ trợ.",
                        cancellationToken);

                    return;
                }

                if (!TryResolveFilePath(
                    request.FileName,
                    out string filePath,
                    out string error))
                {
                    await SendFailureAsync(
                        stream,
                        request.FileName,
                        error,
                        cancellationToken);

                    return;
                }

                if (!File.Exists(filePath))
                {
                    await SendFailureAsync(
                        stream,
                        request.FileName,
                        "File không tồn tại trên Server.",
                        cancellationToken);

                    return;
                }

                await using FileStream fileStream =
                    new FileStream(
                        filePath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read,
                        FileBufferSize,
                        FileOptions.Asynchronous |
                        FileOptions.SequentialScan);

                long fileSize = fileStream.Length;

                var response = new DownloadResponse
                {
                    Success = true,
                    Message =
                        "Server chấp nhận yêu cầu tải file.",
                    FileName = request.FileName,
                    FileSize = fileSize
                };

                await NetworkProtocol.WriteMessageAsync(
                    stream,
                    response,
                    cancellationToken);

                byte[] buffer =
                    new byte[FileBufferSize];

                long totalSent = 0;

                while (true)
                {
                    int bytesRead =
                        await fileStream.ReadAsync(
                            buffer.AsMemory(
                                0,
                                buffer.Length),
                            cancellationToken);

                    if (bytesRead == 0)
                        break;

                    await stream.WriteAsync(
                        buffer.AsMemory(
                            0,
                            bytesRead),
                        cancellationToken);

                    totalSent += bytesRead;

                    if (_throttleMilliseconds > 0)
                    {
                        await Task.Delay(
                            _throttleMilliseconds,
                            cancellationToken);
                    }
                }

                await stream.FlushAsync(
                    cancellationToken);

                LogClient(
                    clientId,
                    $"Hoàn tất '{request.FileName}', đã gửi {totalSent:N0}/{fileSize:N0} byte.");
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                LogClient(
                    clientId,
                    "Dừng xử lý do Server đang tắt.");
            }
            catch (Exception ex)
            {
                LogClient(
                    clientId,
                    $"Lỗi: {ex.Message}");
            }
            finally
            {
                LogClient(
                    clientId,
                    "Đã ngắt kết nối.");
            }
        }
    }

    private bool TryResolveFilePath(
        string? fileName,
        out string fullPath,
        out string error)
    {
        fullPath = string.Empty;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(fileName))
        {
            error = "Tên file không được để trống.";
            return false;
        }

        if (Path.IsPathRooted(fileName))
        {
            error =
                "Không chấp nhận đường dẫn tuyệt đối.";
            return false;
        }

        if (fileName.Contains('/') ||
            fileName.Contains('\\'))
        {
            error =
                "FileName chỉ được chứa tên file.";
            return false;
        }

        if (fileName.Contains(".."))
        {
            error =
                "Tên file không được chứa '..'.";
            return false;
        }

        string rootPath =
            Path.GetFullPath(_serverFilesDirectory);

        string rootPrefix =
            rootPath.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        string candidatePath =
            Path.GetFullPath(
                Path.Combine(
                    rootPath,
                    fileName));

        if (!candidatePath.StartsWith(
            rootPrefix,
            StringComparison.OrdinalIgnoreCase))
        {
            error =
                "Đường dẫn file không hợp lệ.";
            return false;
        }

        fullPath = candidatePath;

        return true;
    }

    private static Task SendFailureAsync(
        NetworkStream stream,
        string? fileName,
        string message,
        CancellationToken cancellationToken)
    {
        var response = new DownloadResponse
        {
            Success = false,
            Message = message,
            FileName = fileName ?? string.Empty,
            FileSize = 0
        };

        return NetworkProtocol.WriteMessageAsync(
            stream,
            response,
            cancellationToken);
    }

    private static void Log(string message)
    {
        Console.WriteLine(
            $"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    private static void LogClient(
        int clientId,
        string message)
    {
        Log($"Client #{clientId}: {message}");
    }
}