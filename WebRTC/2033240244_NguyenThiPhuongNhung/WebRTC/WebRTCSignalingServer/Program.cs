using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Bật hỗ trợ WebSocket
app.UseWebSockets();

const int BufferSize = 1024 * 4;
List<WebSocket> connectedClients = new List<WebSocket>();

// Khắc phục cảnh báo ASP0014: Sử dụng top-level route registration thay vì UseEndpoints
app.MapGet("/signal", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await HandleWebSocketSignaling(webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

async Task HandleWebSocketSignaling(WebSocket webSocket)
{
    connectedClients.Add(webSocket);
    var buffer = new byte[BufferSize];
    try
    {
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            // Xử lý khi client đóng kết nối
            if (result.MessageType == WebSocketMessageType.Close)
            {
                break;
            }

            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Console.WriteLine($"Received: {message}");

            // Phân loại và chuyển tiếp thông điệp
            try
            {
                var signalingMessage = JsonSerializer.Deserialize<SignalingMessage>(message);
                if (signalingMessage != null)
                {
                    await ForwardSignalingMessage(signalingMessage, webSocket);
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Parse Error: {ex.Message}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    finally
    {
        connectedClients.Remove(webSocket);
        if (webSocket.State == WebSocketState.Open)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
    }
}

async Task ForwardSignalingMessage(SignalingMessage signalingMessage, WebSocket sender)
{
    var json = JsonSerializer.Serialize(signalingMessage);
    var bytes = Encoding.UTF8.GetBytes(json);

    foreach (var client in connectedClients)
    {
        if (client != sender && client.State == WebSocketState.Open)
        {
            await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}

app.Run();

// Khắc phục cảnh báo CS8618: Thêm dấu '?' để cho phép giá trị null (nullable reference types)
public class SignalingMessage
{
    public string? Type { get; set; }
    public string? Payload { get; set; }
}