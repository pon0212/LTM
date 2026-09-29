using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

public class FileServer
{
    private readonly IPAddress _ipAddress;
    private readonly int _port;
    private readonly string _sharedFolder;
    private readonly int _maxClients;

    private TcpListener? _listener;
    private volatile bool _running;

    private readonly SemaphoreSlim _clientLimit;

    private static readonly object _logLock = new object();

    public FileServer(
        IPAddress ipAddress,
        int port,
        string sharedFolder,
        int maxClients)
    {
        _ipAddress = ipAddress;
        _port = port;
        _sharedFolder = Path.GetFullPath(sharedFolder);
        _maxClients = maxClients;

        _clientLimit = new SemaphoreSlim(maxClients, maxClients);
    }

    public void Start()
    {
        Directory.CreateDirectory(_sharedFolder);

        _listener = new TcpListener(_ipAddress, _port);
        _listener.Start();

        _running = true;

        Console.WriteLine();
        Console.WriteLine("===== FILE SERVER =====");
        Console.WriteLine($"Dia chi       : {_ipAddress}:{_port}");
        Console.WriteLine($"Thu muc share : {_sharedFolder}");
        Console.WriteLine($"Max Clients   : {_maxClients}");
        Console.WriteLine($"Bat dau luc   : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine("=======================");
        Console.WriteLine();
        Console.WriteLine("Server dang cho client ket noi...");
        Console.WriteLine("Nhan Ctrl+C de dung server.");
        Console.WriteLine();

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            Stop();
        };

        while (_running)
        {
            try
            {
                TcpClient client = _listener.AcceptTcpClient();

                if (!_clientLimit.Wait(0))
                {
                    SendError(
                        client,
                        "SERVER_BUSY",
                        "Server has reached the maximum number of clients."
                    );

                    client.Close();
                    continue;
                }

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    HandleClient(client);
                });
            }
            catch (SocketException)
            {
                if (!_running)
                    break;
            }
            catch (ObjectDisposedException)
            {
                if (!_running)
                    break;
            }
            catch (Exception ex)
            {
                if (_running)
                {
                    WriteLog(
                        "-",
                        "SERVER",
                        "ERROR",
                        0,
                        0,
                        ex.Message
                    );
                }
            }
        }

        Console.WriteLine("Server da dung.");
    }

    private void HandleClient(TcpClient client)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        string endpoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";

        string command = "-";

        long bytesSent = 0;

        string result = "FAILED";

        string detail = "";

        try
        {
            client.ReceiveTimeout = 10000;
            client.SendTimeout = 10000;

            using NetworkStream stream = client.GetStream();

            string? request = ReadLineUtf8(stream);

            if (request == null)
            {
                result = "INVALID_COMMAND";
                detail = "Client did not send a request.";

                SendError(
                    stream,
                    "INVALID_COMMAND",
                    detail
                );

                return;
            }

            command = request;

            if (!request.StartsWith("GET ", StringComparison.Ordinal))
            {
                result = "INVALID_COMMAND";
                detail = "Request format must be GET <RelativePath>.";

                SendError(
                    stream,
                    "INVALID_COMMAND",
                    detail
                );

                return;
            }

            string relativePath = request.Substring(4).Trim();

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                result = "INVALID_COMMAND";
                detail = "File path cannot be empty.";

                SendError(
                    stream,
                    "INVALID_COMMAND",
                    detail
                );

                return;
            }

            if (!TryGetSafeFilePath(relativePath, out string? fullPath))
            {
                result = "INVALID_PATH";
                detail = "The requested path is outside the shared directory.";

                SendError(
                    stream,
                    "INVALID_PATH",
                    detail
                );

                return;
            }

            if (!File.Exists(fullPath))
            {
                result = "FILE_NOT_FOUND";
                detail = "The requested file does not exist.";

                SendError(
                    stream,
                    "FILE_NOT_FOUND",
                    detail
                );

                return;
            }

            FileInfo fileInfo = new FileInfo(fullPath);

            string sha256 = ComputeSha256(fullPath);

            SendOkHeader(
                stream,
                fileInfo.Length,
                sha256
            );

            bytesSent = SendFile(
                stream,
                fullPath
            );

            result = "OK";
            detail = sha256;
        }
        catch (IOException ex)
        {
            result = "IO_ERROR";
            detail = ex.Message;
        }
        catch (SocketException ex)
        {
            result = "SOCKET_ERROR";
            detail = ex.Message;
        }
        catch (Exception ex)
        {
            result = "ERROR";
            detail = ex.Message;
        }
        finally
        {
            stopwatch.Stop();

            try
            {
                client.Close();
            }
            catch
            {
            }

            _clientLimit.Release();

            WriteLog(
                endpoint,
                command,
                result,
                bytesSent,
                stopwatch.ElapsedMilliseconds,
                detail
            );
        }
    }

    private bool TryGetSafeFilePath(
        string relativePath,
        out string? fullPath)
    {
        fullPath = null;

        try
        {
            if (Path.IsPathRooted(relativePath))
            {
                return false;
            }

            string candidatePath =
                Path.GetFullPath(
                    Path.Combine(_sharedFolder, relativePath)
                );

            string rootWithSeparator =
                _sharedFolder.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar
                )
                + Path.DirectorySeparatorChar;

            if (!candidatePath.StartsWith(
                rootWithSeparator,
                StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            fullPath = candidatePath;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string ComputeSha256(string filePath)
    {
        using SHA256 sha256 = SHA256.Create();

        using FileStream fileStream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read
        );

        byte[] hash = sha256.ComputeHash(fileStream);

        return Convert.ToHexString(hash);
    }

    private long SendFile(
        NetworkStream stream,
        string filePath)
    {
        using FileStream fileStream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read
        );

        byte[] buffer = new byte[8192];

        long totalSent = 0;

        while (true)
        {
            int bytesRead = fileStream.Read(
                buffer,
                0,
                buffer.Length
            );

            if (bytesRead <= 0)
                break;

            stream.Write(
                buffer,
                0,
                bytesRead
            );

            totalSent += bytesRead;
        }

        stream.Flush();

        return totalSent;
    }

    private void SendOkHeader(
        NetworkStream stream,
        long fileSize,
        string sha256)
    {
        string header =
            "OK\n" +
            fileSize + "\n" +
            sha256 + "\n";

        byte[] data = Encoding.UTF8.GetBytes(header);

        stream.Write(
            data,
            0,
            data.Length
        );

        stream.Flush();
    }

    private void SendError(
        TcpClient client,
        string errorCode,
        string message)
    {
        try
        {
            using NetworkStream stream = client.GetStream();

            SendError(
                stream,
                errorCode,
                message
            );
        }
        catch
        {
        }
    }

    private void SendError(
        NetworkStream stream,
        string errorCode,
        string message)
    {
        string response =
            "FAILED\n" +
            errorCode + "\n" +
            message + "\n";

        byte[] data = Encoding.UTF8.GetBytes(response);

        stream.Write(
            data,
            0,
            data.Length
        );

        stream.Flush();
    }

    private string? ReadLineUtf8(NetworkStream stream)
    {
        using MemoryStream memory = new MemoryStream();

        while (true)
        {
            int value = stream.ReadByte();

            if (value == -1)
            {
                if (memory.Length == 0)
                    return null;

                break;
            }

            if (value == '\n')
                break;

            if (value != '\r')
            {
                memory.WriteByte((byte)value);
            }

            if (memory.Length > 4096)
            {
                throw new IOException(
                    "Request line is too long."
                );
            }
        }

        return Encoding.UTF8.GetString(
            memory.ToArray()
        );
    }

    private void WriteLog(
        string endpoint,
        string command,
        string result,
        long bytes,
        long elapsedMs,
        string detail)
    {
        lock (_logLock)
        {
            Console.WriteLine(
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] " +
                $"[Thread {Environment.CurrentManagedThreadId}] " +
                $"[{endpoint}] " +
                $"{command} | {result} | " +
                $"{bytes} bytes | " +
                $"{elapsedMs} ms"
            );

            if (!string.IsNullOrWhiteSpace(detail) &&
                result != "OK")
            {
                Console.WriteLine(
                    $"  -> {detail}"
                );
            }
        }
    }

    public void Stop()
    {
        if (!_running)
            return;

        _running = false;

        Console.WriteLine();
        Console.WriteLine("Server dang dung...");

        try
        {
            _listener?.Stop();
        }
        catch
        {
        }
    }
}