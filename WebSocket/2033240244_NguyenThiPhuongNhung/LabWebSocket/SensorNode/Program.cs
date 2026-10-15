using System.Collections.Generic;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;

Console.WriteLine("Khởi động hệ thống mô phỏng cảm biến...");
var rng = new Random();

var tasks = new List<Task> {
    SimulateSensorAsync("A101"),
    SimulateSensorAsync("B202"),
    SimulateSensorAsync("C303")
};
await Task.WhenAll(tasks);

async Task SimulateSensorAsync(string roomName)
{
    int reconnectDelay = 1000;
    while (true)
    {
        using var client = new ClientWebSocket();
        try
        {
            await client.ConnectAsync(new Uri("ws://localhost:5000/sensor"), CancellationToken.None);
            Console.WriteLine($"[Trạm {roomName}] Đã kết nối thành công.");
            reconnectDelay = 1000;

            while (client.State == WebSocketState.Open)
            {
                var payload = new
                {
                    room = roomName,
                    temperature = Math.Round(20.0 + rng.NextDouble() * 10.0, 2),
                    timestamp = DateTime.Now
                };
                string jsonMsg = JsonSerializer.Serialize(payload);
                var bytes = Encoding.UTF8.GetBytes(jsonMsg);

                await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                await Task.Delay(3000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Trạm {roomName}] Lỗi mạng: {ex.Message}. Thử lại sau {reconnectDelay / 1000}s...");
            await Task.Delay(reconnectDelay);
            reconnectDelay = Math.Min(reconnectDelay * 2, 30000);
        }
    }
}