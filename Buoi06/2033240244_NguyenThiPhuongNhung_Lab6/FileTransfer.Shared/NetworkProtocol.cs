using System.Buffers.Binary;
using System.Text.Json;

namespace FileTransfer.Shared;

public static class NetworkProtocol
{
    public const int MaxJsonSize = 64 * 1024;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

    public static async Task WriteMessageAsync<T>(
        Stream stream,
        T message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(message);

        byte[] jsonBytes =
            JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);

        if (jsonBytes.Length == 0 || jsonBytes.Length > MaxJsonSize)
        {
            throw new InvalidDataException(
                $"Kích thước JSON phải nằm trong khoảng 1..{MaxJsonSize} byte.");
        }

        byte[] lengthPrefix = new byte[sizeof(int)];

        BinaryPrimitives.WriteInt32BigEndian(
            lengthPrefix,
            jsonBytes.Length);

        await stream.WriteAsync(
            lengthPrefix.AsMemory(),
            cancellationToken);

        await stream.WriteAsync(
            jsonBytes.AsMemory(),
            cancellationToken);

        await stream.FlushAsync(cancellationToken);
    }

    public static async Task<T> ReadMessageAsync<T>(
        Stream stream,
        CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(stream);

        byte[] lengthPrefix = new byte[sizeof(int)];

        await stream.ReadExactlyAsync(
            lengthPrefix.AsMemory(),
            cancellationToken);

        int jsonLength =
            BinaryPrimitives.ReadInt32BigEndian(lengthPrefix);

        if (jsonLength <= 0 || jsonLength > MaxJsonSize)
        {
            throw new InvalidDataException(
                $"Độ dài JSON không hợp lệ: {jsonLength} byte. " +
                $"Giới hạn cho phép là 1..{MaxJsonSize} byte.");
        }

        byte[] jsonBytes = new byte[jsonLength];

        await stream.ReadExactlyAsync(
            jsonBytes.AsMemory(),
            cancellationToken);

        try
        {
            T? message =
                JsonSerializer.Deserialize<T>(
                    jsonBytes,
                    JsonOptions);

            return message ?? throw new InvalidDataException(
                "JSON hợp lệ về cú pháp nhưng không tạo được đối tượng.");
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException(
                "Nội dung JSON không hợp lệ.",
                ex);
        }
    }
}