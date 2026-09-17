using System.Net;
using System.Net.Sockets;
using Protocol;

namespace FileSocketServer.Services;

public sealed class FileServer
{
    private TcpListener? _listener;
    private readonly object _clientLock = new();
    private readonly HashSet<TcpClient> _clients = new();

    public event Action<string>? Log;
    public event Action<IReadOnlyList<string>>? ClientsChanged;

    public bool IsRunning => _listener is not null;

    public async Task RunAsync(
        IPAddress ipAddress,
        int port,
        string serverFolder,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(serverFolder);

        string welcomePath = Path.Combine(serverFolder, "Welcome.txt");
        if (!File.Exists(welcomePath))
        {
            await File.WriteAllTextAsync(
                welcomePath,
                "Xin chao! Day la file Welcome.txt cua Lab 5.",
                cancellationToken);
        }

        _listener = new TcpListener(ipAddress, port);
        _listener.Start();

        OnLog($"Server dang lang nghe tai {ipAddress}:{port}");
        OnLog($"Thu muc chia se: {serverFolder}");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync(cancellationToken);

                lock (_clientLock)
                {
                    _clients.Add(client);
                    RaiseClientsChanged();
                }

                _ = Task.Run(
                    () => HandleClientAsync(client, serverFolder, cancellationToken),
                    CancellationToken.None);
            }
        }
        catch (OperationCanceledException)
        {
            OnLog("Server da nhan lenh dung.");
        }
        finally
        {
            Stop();
        }
    }

    public void Stop()
    {
        try { _listener?.Stop(); } catch { }
        _listener = null;

        lock (_clientLock)
        {
            foreach (TcpClient c in _clients.ToArray())
            {
                try { c.Close(); } catch { }
            }

            _clients.Clear();
            RaiseClientsChanged();
        }
    }

    private async Task HandleClientAsync(
        TcpClient client,
        string serverFolder,
        CancellationToken serverToken)
    {
        string remote = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
        OnLog($"Client ket noi: {remote}");

        try
        {
            await using NetworkStream stream = client.GetStream();

            DownloadRequest request =
                await NetworkProtocol.ReadJsonAsync<DownloadRequest>(stream, serverToken);

            OnLog($"{remote} yeu cau: {request.Command} {request.FileName}");

            if (!string.Equals(request.Command, "DOWNLOAD", StringComparison.OrdinalIgnoreCase))
            {
                await SendErrorAsync(stream, request.FileName, "Lenh khong hop le.", serverToken);
                return;
            }

            if (!IsSafeFileName(request.FileName))
            {
                await SendErrorAsync(stream, request.FileName, "Ten file/duong dan khong hop le.", serverToken);
                return;
            }

            string fullPath = Path.Combine(serverFolder, request.FileName);

            if (!File.Exists(fullPath))
            {
                await SendErrorAsync(stream, request.FileName, "File khong ton tai.", serverToken);
                return;
            }

            FileInfo fileInfo = new(fullPath);

            DownloadResponse response = new(
                true,
                "San sang gui file.",
                fileInfo.Name,
                fileInfo.Length
            );

            await NetworkProtocol.WriteJsonAsync(stream, response, serverToken);

            await using FileStream fileStream = new(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                useAsync: true);

            await fileStream.CopyToAsync(stream, 81920, serverToken);
            await stream.FlushAsync(serverToken);

            OnLog($"Gui thanh cong {fileInfo.Name} ({fileInfo.Length} byte) cho {remote}");
        }
        catch (OperationCanceledException)
        {
            OnLog($"Huy phuc vu Client: {remote}");
        }
        catch (EndOfStreamException ex)
        {
            OnLog($"Client {remote} dong som: {ex.Message}");
        }
        catch (IOException ex)
        {
            OnLog($"Loi I/O voi {remote}: {ex.Message}");
        }
        catch (SocketException ex)
        {
            OnLog($"Loi socket voi {remote}: {ex.Message}");
        }
        catch (Exception ex)
        {
            OnLog($"Loi voi {remote}: {ex.Message}");
        }
        finally
        {
            try { client.Close(); } catch { }

            lock (_clientLock)
            {
                _clients.Remove(client);
                RaiseClientsChanged();
            }

            OnLog($"Client ngat ket noi: {remote}");
        }
    }

    private static bool IsSafeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        if (Path.IsPathRooted(fileName))
            return false;

        if (fileName.Contains('/') || fileName.Contains('\\') || fileName.Contains(".."))
            return false;

        return string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal);
    }

    private static async Task SendErrorAsync(
        NetworkStream stream,
        string fileName,
        string message,
        CancellationToken cancellationToken)
    {
        DownloadResponse response = new(
            false,
            message,
            fileName ?? string.Empty,
            0
        );

        await NetworkProtocol.WriteJsonAsync(stream, response, cancellationToken);
    }

    private void OnLog(string message)
        => Log?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");

    private void RaiseClientsChanged()
    {
        var clients = _clients
            .Select(c => c.Client.RemoteEndPoint?.ToString() ?? "Unknown")
            .ToArray();

        ClientsChanged?.Invoke(clients);
    }
}
