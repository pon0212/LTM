using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseWebSockets();

var viewers = new ConcurrentDictionary<WebSocket, string>();
var sensorHeartbeats = new ConcurrentDictionary<string, DateTime>();

_ = Task.Run(async () => {
    while (true)
    {
        var cutoffTime = DateTime.Now.AddSeconds(-15);
        foreach (var sensor in sensorHeartbeats)
        {
            if (sensor.Value < cutoffTime)
            {
                Console.WriteLine($"[Cảnh báo] Trạm {sensor.Key} không phản hồi!");
                sensorHeartbeats.TryRemove(sensor.Key, out _);
            }
        }
        await Task.Delay(5000);
    }
});

app.Use(async (context, next) => {
    if (context.Request.Path == "/sensor" && context.WebSockets.IsWebSocketRequest)
    {
        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        await ProcessSensorData(ws);
    }
    else if (context.Request.Path == "/dashboard" && context.WebSockets.IsWebSocketRequest)
    {
        string targetRoom = context.Request.Query["room"].ToString() ?? "";
        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        viewers.TryAdd(ws, targetRoom);
        await KeepDashboardConnectionOpen(ws);
        viewers.TryRemove(ws, out _);
    }
    else
    {
        await next();
    }
});

async Task ProcessSensorData(WebSocket socket)
{
    var buffer = new byte[2048];
    try
    {
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close) break;

            string jsonContent = Encoding.UTF8.GetString(buffer, 0, result.Count);
            using var doc = JsonDocument.Parse(jsonContent);
            string roomName = doc.RootElement.GetProperty("room").GetString() ?? "Unknown";

            sensorHeartbeats[roomName] = DateTime.Now;
            Console.WriteLine($"[Server] Nhận dữ liệu từ {roomName}");

            var outBuffer = Encoding.UTF8.GetBytes(jsonContent);
            foreach (var viewer in viewers)
            {
                if (viewer.Key.State == WebSocketState.Open)
                {
                    if (string.IsNullOrEmpty(viewer.Value) || viewer.Value == roomName)
                    {
                        await viewer.Key.SendAsync(new ArraySegment<byte>(outBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
        }
    }
    catch (Exception ex) { Console.WriteLine($"Lỗi: {ex.Message}"); }
}

async Task KeepDashboardConnectionOpen(WebSocket socket)
{
    var buffer = new byte[1024];
    while (socket.State == WebSocketState.Open)
    {
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        if (result.MessageType == WebSocketMessageType.Close) break;
    }
}

app.Run("http://localhost:5000");