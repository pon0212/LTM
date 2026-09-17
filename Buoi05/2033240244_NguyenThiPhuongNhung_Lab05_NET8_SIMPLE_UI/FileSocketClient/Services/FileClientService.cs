using System.Net.Sockets;
using Protocol;

namespace FileSocketClient.Services;

public sealed record DownloadProgress(long ReceivedBytes, long TotalBytes)
{
    public double Percent => TotalBytes == 0 ? 100 : ReceivedBytes * 100.0 / TotalBytes;
}

public sealed class FileClientService
{
    public async Task<DownloadResponse> DownloadAsync(
        string serverIp,
        int port,
        string remoteFile,
        string savePath,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        using TcpClient client = new();

        await client.ConnectAsync(serverIp, port, cancellationToken);

        await using NetworkStream stream = client.GetStream();

        DownloadRequest request = new("DOWNLOAD", remoteFile);
        await NetworkProtocol.WriteJsonAsync(stream, request, cancellationToken);

        DownloadResponse response =
            await NetworkProtocol.ReadJsonAsync<DownloadResponse>(stream, cancellationToken);

        if (!response.Success)
            return response;

        string fullSavePath = Path.GetFullPath(savePath);
        string? directory = Path.GetDirectoryName(fullSavePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        string tempPath = fullSavePath + ".part";
        if (File.Exists(tempPath))
            File.Delete(tempPath);

        try
        {
            await using FileStream fileStream = new(
                tempPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true);

            long remaining = response.FileSize;
            long received = 0;
            byte[] buffer = new byte[81920];

            while (remaining > 0)
            {
                int read = await stream.ReadAsync(
                    buffer.AsMemory(0, (int)Math.Min(buffer.Length, remaining)),
                    cancellationToken);

                if (read == 0)
                    throw new EndOfStreamException(
                        $"Ket noi dong som. Con thieu {remaining} byte.");

                await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);

                received += read;
                remaining -= read;
                progress?.Report(new DownloadProgress(received, response.FileSize));
            }

            await fileStream.FlushAsync(cancellationToken);

            if (File.Exists(fullSavePath))
                File.Delete(fullSavePath);

            File.Move(tempPath, fullSavePath);
            progress?.Report(new DownloadProgress(response.FileSize, response.FileSize));

            return response;
        }
        catch
        {
            try
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
            catch { }

            throw;
        }
    }
}
