using System;
using System.ComponentModel;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DashboardUI
{
    public partial class Form1 : Form
    {
        private ClientWebSocket _webSocket;
        private DataGridView gridData;
        private BindingList<DataRecord> _records;

        public Form1()
        {
            this.Text = "Hệ thống giám sát WebSocket";
            this.Size = new System.Drawing.Size(600, 400);

            _records = new BindingList<DataRecord>();
            gridData = new DataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = _records,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false
            };
            this.Controls.Add(gridData);

            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            _webSocket = new ClientWebSocket();
            try
            {
                Uri serverUri = new Uri("ws://localhost:5000/dashboard");
                await _webSocket.ConnectAsync(serverUri, CancellationToken.None);
                _ = ReceiveDataFromServer();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể kết nối tới server: {ex.Message}");
            }
        }

        private async Task ReceiveDataFromServer()
        {
            var buffer = new byte[2048];
            try
            {
                while (_webSocket.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        var reading = JsonSerializer.Deserialize<DataRecord>(json);

                        if (reading != null)
                        {
                            this.Invoke((MethodInvoker)delegate {
                                _records.Insert(0, reading);
                                if (_records.Count > 100) _records.RemoveAt(_records.Count - 1);
                            });
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client exit", CancellationToken.None);
                _webSocket.Dispose();
            }
        }
    }

    public class DataRecord
    {
        public string room { get; set; }
        public double temperature { get; set; }
        public DateTime timestamp { get; set; }
    }
}