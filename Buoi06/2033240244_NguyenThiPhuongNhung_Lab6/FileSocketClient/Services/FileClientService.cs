using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FileTransfer.Shared;

namespace FileSocketClient.Services;

public sealed class FileClientService
{
    private const int FileBufferSize = 81_920;

    public async Task<DownloadResponse> DownloadFileAsync(
        IPAddress serverAddress,
        int port,
        string remoteFileName,
        string destinationPath,
        IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serverAddress);

        if (port is < 1 or > 65_535)
            throw new ArgumentOutOfRangeException(nameof(port));

        if (string.IsNullOrWhiteSpace(remoteFileName))
            throw new ArgumentException("Tên file không được để trống.", nameof(remoteFileName));

        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentException("Đường dẫn lưu không được để trống.", nameof(destinationPath));

        string temporaryPath = destinationPath + ".part";
        bool downloadCompleted = false;

        try
        {
            using var client = new TcpClient
            {
                NoDelay = true
            };

            await client.ConnectAsync(serverAddress, port, cancellationToken);

            using NetworkStream stream = client.GetStream();

            var request = new DownloadRequest
            {
                Command = "DOWNLOAD",
                FileName = remoteFileName
            };

            await NetworkProtocol.WriteMessageAsync(
                stream,
                request,
                cancellationToken);

            DownloadResponse response =
                await NetworkProtocol.ReadMessageAsync<DownloadResponse>(
                    stream,
                    cancellationToken);

            if (!response.Success)
                return response;

            if (response.FileSize < 0)
                throw new InvalidDataException("Server trả về FileSize âm.");

            string? destinationDirectory = Path.GetDirectoryName(destinationPath);

            if (string.IsNullOrWhiteSpace(destinationDirectory) ||
                !Directory.Exists(destinationDirectory))
                throw new DirectoryNotFoundException("Thư mục lưu file không tồn tại.");

            progress?.Report(0);

            await using (var outputStream = new FileStream(
                temporaryPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                FileBufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                byte[] buffer = new byte[FileBufferSize];
                long totalReceived = 0;

                while (totalReceived < response.FileSize)
                {
                    int bytesToRead = (int)Math.Min(
                        buffer.Length,
                        response.FileSize - totalReceived);

                    int bytesRead;

                    try
                    {
                        bytesRead = await stream.ReadAsync(
                            buffer.AsMemory(0, bytesToRead),
                            cancellationToken);
                    }
                    catch (IOException ex)
                    {
                        throw new EndOfStreamException(
                            $"Server ngắt kết nối, thiếu dữ liệu. Đã nhận {totalReceived:N0}/{response.FileSize:N0} byte.",
                            ex);
                    }

                    if (bytesRead == 0)
                    {
                        throw new EndOfStreamException(
                            $"Server ngắt kết nối, thiếu dữ liệu. Đã nhận {totalReceived:N0}/{response.FileSize:N0} byte.");
                    }

                    await outputStream.WriteAsync(
                        buffer.AsMemory(0, bytesRead),
                        cancellationToken);

                    totalReceived += bytesRead;

                    double percent = response.FileSize == 0
                        ? 100
                        : totalReceived * 100.0 / response.FileSize;

                    progress?.Report(percent);
                }

                await outputStream.FlushAsync(cancellationToken);
            }

            File.Move(temporaryPath, destinationPath, overwrite: true);

            downloadCompleted = true;
            progress?.Report(100);

            return response;
        }
        finally
        {
            if (!downloadCompleted)
                TryDeletePartialFile(temporaryPath);
        }
    }

    private static void TryDeletePartialFile(string temporaryPath)
    {
        try
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}