using System.Buffers.Binary;
using System.Text.Json;

namespace Protocol;

public record DownloadRequest(string Command, string FileName);

public record DownloadResponse(
    bool Success,
    string Message,
    string FileName,
    long FileSize
);

public static class NetworkProtocol
{
    private const int HeaderSize = 4;
    private const int MaxJsonLength = 1024 * 1024;

    public static async Task WriteJsonAsync<T>(
        Stream stream,
        T value,
        CancellationToken cancellationToken = default)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(value);
        if (json.Length <= 0 || json.Length > MaxJsonLength)
            throw new InvalidDataException("Kich thuoc JSON khong hop le.");

        byte[] header = new byte[HeaderSize];
        BinaryPrimitives.WriteInt32BigEndian(header, json.Length);

        await stream.WriteAsync(header, cancellationToken);
        await stream.WriteAsync(json, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    public static async Task<T> ReadJsonAsync<T>(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        byte[] header = new byte[HeaderSize];
        await ReadExactlyAsync(stream, header, cancellationToken);

        int jsonLength = BinaryPrimitives.ReadInt32BigEndian(header);
        if (jsonLength <= 0 || jsonLength > MaxJsonLength)
            throw new InvalidDataException("Do dai JSON khong hop le.");

        byte[] json = new byte[jsonLength];
        await ReadExactlyAsync(stream, json, cancellationToken);

        T? value = JsonSerializer.Deserialize<T>(json);
        return value ?? throw new InvalidDataException("JSON khong hop le.");
    }

    public static async Task ReadExactlyAsync(
        Stream stream,
        Memory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        int offset = 0;

        while (offset < buffer.Length)
        {
            int read = await stream.ReadAsync(buffer[offset..], cancellationToken);
            if (read == 0)
                throw new EndOfStreamException("Ket noi dong truoc khi nhan du du lieu.");

            offset += read;
        }
    }
}
