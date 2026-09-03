using System.Net.Sockets;
using System.Text;
using Chat.Shared.Protocol;

namespace Chat.Server.Networking;

internal sealed class ClientSession : IDisposable
{
    private readonly NetworkStream _stream;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private readonly object _sendLock = new();
    private int _disposed;

    public ClientSession(TcpClient tcpClient)
    {
        TcpClient = tcpClient;

        _stream = tcpClient.GetStream();

        var utf8 = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false);

        _reader = new StreamReader(
            _stream,
            utf8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 4096,
            leaveOpen: true);

        _writer = new StreamWriter(
            _stream,
            utf8,
            bufferSize: 4096,
            leaveOpen: true)
        {
            AutoFlush = true
        };
    }

    public TcpClient TcpClient { get; }

    public string Username { get; set; } = string.Empty;

    public ChatMessage? ReadMessage()
    {
        string? json = _reader.ReadLine();

        return json is null
            ? null
            : ProtocolSerializer.Deserializer(json);
    }

    public void SendMessage(ChatMessage message)
    {
        string json =
            ProtocolSerializer.Serializer(message);

        lock (_sendLock)
        {
            ObjectDisposedException.ThrowIf(
                Volatile.Read(ref _disposed) == 1,
                this);

            _writer.WriteLine(json);
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(
                ref _disposed, 1) == 1)
        {
            return;
        }

        try
        {
            TcpClient.Client.Shutdown(
                SocketShutdown.Both);
        }
        catch { }

        try { _reader.Dispose(); } catch { }
        try { _writer.Dispose(); } catch { }
        try { _stream.Dispose(); } catch { }
        try { TcpClient.Dispose(); } catch { }
    }
}