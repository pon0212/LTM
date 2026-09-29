using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

public class FileDownloader
{
    private static readonly object _consoleLock = new object();

    public DownloadResult DownloadFile(DownloadRequest request)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        DownloadResult result = new DownloadResult
        {
            Server = $"{request.Host}:{request.Port}",
            RemoteFile = request.RemoteFile,
            LocalFile = request.LocalFile,
            Status = "FAILED"
        };

        string partFile = request.LocalFile + ".part";

        try
        {
            if (request.RemoteFile.Contains('\r') ||
                request.RemoteFile.Contains('\n'))
            {
                result.Status = "INVALID_REMOTE_PATH";
                return result;
            }

            string? directory = Path.GetDirectoryName(request.LocalFile);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(partFile))
            {
                File.Delete(partFile);
            }

            using TcpClient client = new TcpClient();

            client.ReceiveTimeout = 10000;
            client.SendTimeout = 10000;

            client.Connect(
                request.Host,
                request.Port
            );

            using NetworkStream stream = client.GetStream();

            SendRequest(
                stream,
                request.RemoteFile
            );

            string? firstLine = ReadLineUtf8(stream);

            if (firstLine == null)
            {
                result.Status = "INVALID_RESPONSE";
                return result;
            }

            if (firstLine == "FAILED")
            {
                string? errorCode = ReadLineUtf8(stream);
                string? errorMessage = ReadLineUtf8(stream);

                result.Status =
                    string.IsNullOrWhiteSpace(errorCode)
                    ? "FAILED"
                    : errorCode;

                lock (_consoleLock)
                {
                    Console.WriteLine();
                    Console.WriteLine(
                        $"[{request.RemoteFile}] THAT BAI"
                    );

                    Console.WriteLine(
                        $"Ma loi: {errorCode}"
                    );

                    Console.WriteLine(
                        $"Thong bao: {errorMessage}"
                    );
                }

                return result;
            }

            if (firstLine != "OK")
            {
                result.Status = "INVALID_RESPONSE";
                return result;
            }

            string? sizeText = ReadLineUtf8(stream);
            string? serverHash = ReadLineUtf8(stream);

            if (!long.TryParse(sizeText, out long fileSize) ||
                fileSize < 0)
            {
                result.Status = "INVALID_FILE_SIZE";
                return result;
            }

            if (!IsValidSha256(serverHash))
            {
                result.Status = "INVALID_SHA256";
                return result;
            }

            result.Size = fileSize;

            string receivedHash = ReceiveFile(
                stream,
                partFile,
                fileSize,
                request.RemoteFile
            );

            if (!string.Equals(
                receivedHash,
                serverHash,
                StringComparison.OrdinalIgnoreCase))
            {
                result.Status = "HASH_MISMATCH";

                SafeDelete(partFile);

                return result;
            }

            if (File.Exists(request.LocalFile))
            {
                File.Delete(request.LocalFile);
            }

            File.Move(
                partFile,
                request.LocalFile
            );

            result.Status = "SUCCESS";

            stopwatch.Stop();

            result.TimeMs =
                stopwatch.ElapsedMilliseconds;

            if (stopwatch.Elapsed.TotalSeconds > 0)
            {
                result.AverageSpeed =
                    fileSize /
                    1024.0 /
                    1024.0 /
                    stopwatch.Elapsed.TotalSeconds;
            }

            lock (_consoleLock)
            {
                Console.WriteLine();
                Console.WriteLine(
                    $"[{request.RemoteFile}] TAI THANH CONG"
                );

                Console.WriteLine(
                    $"Da luu: {request.LocalFile}"
                );
            }

            return result;
        }
        catch (SocketException ex)
        {
            result.Status = "CONNECTION_ERROR";

            lock (_consoleLock)
            {
                Console.WriteLine(
                    $"Loi ket noi {request.Host}:{request.Port}: {ex.Message}"
                );
            }

            return result;
        }
        catch (IOException ex)
        {
            result.Status = "IO_ERROR";

            lock (_consoleLock)
            {
                Console.WriteLine(
                    $"Loi I/O: {ex.Message}"
                );
            }

            return result;
        }
        catch (Exception ex)
        {
            result.Status = "ERROR";

            lock (_consoleLock)
            {
                Console.WriteLine(
                    $"Loi: {ex.Message}"
                );
            }

            return result;
        }
        finally
        {
            stopwatch.Stop();

            if (result.TimeMs == 0)
            {
                result.TimeMs =
                    stopwatch.ElapsedMilliseconds;
            }

            if (result.Status != "SUCCESS")
            {
                SafeDelete(partFile);
            }
        }
    }

    private void SendRequest(
        NetworkStream stream,
        string remoteFile)
    {
        string request =
            $"GET {remoteFile}\n";

        byte[] data =
            Encoding.UTF8.GetBytes(request);

        stream.Write(
            data,
            0,
            data.Length
        );

        stream.Flush();
    }

    private string ReceiveFile(
        NetworkStream stream,
        string partFile,
        long fileSize,
        string displayName)
    {
        using SHA256 sha256 =
            SHA256.Create();

        using FileStream fileStream =
            new FileStream(
                partFile,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );

        byte[] buffer =
            new byte[8192];

        long received = 0;

        Stopwatch totalTimer =
            Stopwatch.StartNew();

        Stopwatch progressTimer =
            Stopwatch.StartNew();

        while (received < fileSize)
        {
            int bytesToRead =
                (int)Math.Min(
                    buffer.Length,
                    fileSize - received
                );

            int bytesRead =
                stream.Read(
                    buffer,
                    0,
                    bytesToRead
                );

            if (bytesRead <= 0)
            {
                throw new IOException(
                    "Server ngat ket noi truoc khi gui du file."
                );
            }

            fileStream.Write(
                buffer,
                0,
                bytesRead
            );

            sha256.TransformBlock(
                buffer,
                0,
                bytesRead,
                null,
                0
            );

            received += bytesRead;

            if (progressTimer.ElapsedMilliseconds >= 500 ||
                received == fileSize)
            {
                ShowProgress(
                    displayName,
                    received,
                    fileSize,
                    totalTimer.Elapsed.TotalSeconds
                );

                progressTimer.Restart();
            }
        }

        sha256.TransformFinalBlock(
            Array.Empty<byte>(),
            0,
            0
        );

        byte[] hash =
            sha256.Hash
            ?? Array.Empty<byte>();

        return Convert.ToHexString(hash);
    }

    private void ShowProgress(
        string fileName,
        long received,
        long total,
        double seconds)
    {
        double percent =
            total == 0
            ? 100
            : received * 100.0 / total;

        double speed =
            seconds <= 0
            ? 0
            : received /
              1024.0 /
              1024.0 /
              seconds;

        lock (_consoleLock)
        {
            Console.WriteLine(
                $"[{fileName}] " +
                $"{FormatFileSize(received)}/" +
                $"{FormatFileSize(total)} - " +
                $"{percent:F1}% - " +
                $"{speed:F2} MB/s"
            );
        }
    }

    private string? ReadLineUtf8(
        NetworkStream stream)
    {
        using MemoryStream memory =
            new MemoryStream();

        while (true)
        {
            int value =
                stream.ReadByte();

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
                memory.WriteByte(
                    (byte)value
                );
            }

            if (memory.Length > 4096)
            {
                throw new IOException(
                    "Response line is too long."
                );
            }
        }

        return Encoding.UTF8.GetString(
            memory.ToArray()
        );
    }

    private bool IsValidSha256(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (value.Length != 64)
            return false;

        foreach (char c in value)
        {
            bool valid =
                char.IsDigit(c) ||
                (c >= 'A' && c <= 'F') ||
                (c >= 'a' && c <= 'f');

            if (!valid)
                return false;
        }

        return true;
    }

    private void SafeDelete(
        string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
        }
    }

    public static string FormatFileSize(
        long bytes)
    {
        if (bytes >= 1024L * 1024 * 1024)
        {
            return
                $"{bytes / 1024.0 / 1024.0 / 1024.0:F2} GB";
        }

        if (bytes >= 1024L * 1024)
        {
            return
                $"{bytes / 1024.0 / 1024.0:F2} MB";
        }

        if (bytes >= 1024L)
        {
            return
                $"{bytes / 1024.0:F2} KB";
        }

        return $"{bytes} B";
    }
}