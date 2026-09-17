using System.Net;
using System.Net.Sockets;
using FileSocketServer.Services;

namespace FileSocketServer;

public sealed class ServerForm : Form
{
    private readonly TextBox txtIp = new();
    private readonly NumericUpDown nudPort = new();
    private readonly TextBox txtFolder = new();
    private readonly Button btnBrowse = new();
    private readonly Button btnStart = new();
    private readonly Button btnStop = new();
    private readonly Label lblStatus = new();
    private readonly RichTextBox rtbLog = new();
    private readonly ListBox lstClients = new();
    private readonly Label lblClientSummary = new();

    private readonly FileServer _server = new();
    private CancellationTokenSource? _cts;

    public ServerForm()
    {
        Text = "TCP File Transfer Server - .NET 8";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(820, 570);
        MinimumSize = new Size(760, 520);
        BackColor = Color.FromArgb(245, 247, 250);
        Font = new Font("Segoe UI", 9.5F);
        AutoScaleMode = AutoScaleMode.Dpi;

        ConfigureControls();
        BuildLayout();
        WireEvents();

        _server.Log += message => Ui(() =>
        {
            rtbLog.AppendText(message + Environment.NewLine);
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.ScrollToCaret();
        });

        _server.ClientsChanged += clients => Ui(() =>
        {
            lstClients.Items.Clear();
            foreach (string client in clients)
                lstClients.Items.Add(client);

            lblClientSummary.Text = $"{clients.Count} thiết bị trực tuyến";
        });
    }

    private void ConfigureControls()
    {
        txtIp.Text = "0.0.0.0";
        txtIp.Dock = DockStyle.Fill;

        nudPort.Minimum = 1;
        nudPort.Maximum = 65535;
        nudPort.Value = 8888;
        nudPort.Dock = DockStyle.Fill;

        txtFolder.Text = Path.Combine(AppContext.BaseDirectory, "ServerFiles");
        txtFolder.ReadOnly = true;
        txtFolder.Dock = DockStyle.Fill;

        btnBrowse.Text = "Chọn thư mục";
        btnBrowse.Dock = DockStyle.Fill;

        btnStart.Text = "Khởi động";
        btnStart.Dock = DockStyle.Fill;
        btnStart.BackColor = Color.FromArgb(22, 163, 74);
        btnStart.ForeColor = Color.White;
        btnStart.FlatStyle = FlatStyle.Flat;

        btnStop.Text = "Dừng";
        btnStop.Dock = DockStyle.Fill;
        btnStop.Enabled = false;
        btnStop.BackColor = Color.FromArgb(220, 38, 38);
        btnStop.ForeColor = Color.White;
        btnStop.FlatStyle = FlatStyle.Flat;

        lblStatus.Text = "● ĐÃ DỪNG";
        lblStatus.Dock = DockStyle.Fill;
        lblStatus.TextAlign = ContentAlignment.MiddleCenter;
        lblStatus.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblStatus.ForeColor = Color.FromArgb(185, 28, 28);

        rtbLog.ReadOnly = true;
        rtbLog.Dock = DockStyle.Fill;
        rtbLog.Font = new Font("Consolas", 9F);
        rtbLog.BackColor = Color.White;

        lstClients.Dock = DockStyle.Fill;
        lstClients.BackColor = Color.White;

        lblClientSummary.Text = "0 thiết bị trực tuyến";
        lblClientSummary.Dock = DockStyle.Fill;
        lblClientSummary.TextAlign = ContentAlignment.MiddleLeft;
    }

    private void BuildLayout()
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 64,
            BackColor = Color.FromArgb(30, 41, 59),
            Padding = new Padding(20, 10, 20, 6)
        };

        var title = new Label
        {
            Text = "TCP FILE TRANSFER SERVER",
            Dock = DockStyle.Top,
            Height = 30,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        var subtitle = new Label
        {
            Text = "Server truyền file qua TCP - Port mặc định 8888",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(203, 213, 225),
            Font = new Font("Segoe UI", 9F)
        };

        header.Controls.Add(subtitle);
        header.Controls.Add(title);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(14),
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.FromArgb(245, 247, 250)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 185));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        root.Controls.Add(BuildConfigGroup(), 0, 0);
        root.Controls.Add(BuildBottomArea(), 0, 1);

        Controls.Add(root);
        Controls.Add(header);
    }

    private Control BuildConfigGroup()
    {
        var group = new GroupBox
        {
            Text = "Cấu hình Server",
            Dock = DockStyle.Fill,
            Padding = new Padding(12)
        };

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 4
        };

        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

        table.Controls.Add(CreateLabel("IP lắng nghe:"), 0, 0);
        table.Controls.Add(txtIp, 1, 0);
        table.Controls.Add(CreateLabel("Cổng:"), 2, 0);
        table.Controls.Add(nudPort, 3, 0);

        table.Controls.Add(CreateLabel("Thư mục:"), 0, 1);
        table.Controls.Add(txtFolder, 1, 1);
        table.SetColumnSpan(txtFolder, 2);
        table.Controls.Add(btnBrowse, 3, 1);

        var hint = new Label
        {
            Text = "Client chỉ được tải các file nằm trong thư mục chia sẻ này.",
            Dock = DockStyle.Fill,
            ForeColor = Color.DimGray,
            TextAlign = ContentAlignment.MiddleLeft
        };
        table.Controls.Add(hint, 1, 2);
        table.SetColumnSpan(hint, 3);

        table.Controls.Add(btnStart, 1, 3);
        table.Controls.Add(btnStop, 2, 3);
        table.Controls.Add(lblStatus, 3, 3);

        group.Controls.Add(table);
        return group;
    }

    private Control BuildBottomArea()
    {
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 520,
            SplitterWidth = 8
        };

        var logGroup = new GroupBox
        {
            Text = "Nhật ký hoạt động",
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        logGroup.Controls.Add(rtbLog);

        var clientGroup = new GroupBox
        {
            Text = "Client đang kết nối",
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };

        var clientTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1
        };
        clientTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        clientTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        clientTable.Controls.Add(lstClients, 0, 0);
        clientTable.Controls.Add(lblClientSummary, 0, 1);
        clientGroup.Controls.Add(clientTable);

        split.Panel1.Controls.Add(logGroup);
        split.Panel2.Controls.Add(clientGroup);

        return split;
    }

    private static Label CreateLabel(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private void WireEvents()
    {
        btnBrowse.Click += (_, _) =>
        {
            using var dialog = new FolderBrowserDialog();

            if (Directory.Exists(txtFolder.Text))
                dialog.SelectedPath = txtFolder.Text;

            if (dialog.ShowDialog(this) == DialogResult.OK)
                txtFolder.Text = dialog.SelectedPath;
        };

        btnStart.Click += async (_, _) => await StartServerAsync();
        btnStop.Click += (_, _) => StopServer();
        FormClosing += (_, _) => StopServer();
    }

    private async Task StartServerAsync()
    {
        if (_cts is not null)
            return;

        if (!IPAddress.TryParse(txtIp.Text.Trim(), out IPAddress? ip))
        {
            MessageBox.Show("Địa chỉ IP không hợp lệ.", "Thông báo");
            return;
        }

        string folder = txtFolder.Text.Trim();

        if (string.IsNullOrWhiteSpace(folder))
        {
            MessageBox.Show("Hãy chọn thư mục chia sẻ.", "Thông báo");
            return;
        }

        Directory.CreateDirectory(folder);

        _cts = new CancellationTokenSource();
        SetRunning(true);

        try
        {
            await _server.RunAsync(ip, (int)nudPort.Value, folder, _cts.Token);
        }
        catch (SocketException ex)
        {
            MessageBox.Show("Không thể khởi động Server:\n" + ex.Message, "Lỗi mạng");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi Server:\n" + ex.Message, "Lỗi");
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            SetRunning(false);
        }
    }

    private void StopServer()
    {
        _cts?.Cancel();
        _server.Stop();
    }

    private void SetRunning(bool running)
    {
        btnStart.Enabled = !running;
        btnStop.Enabled = running;
        txtIp.Enabled = !running;
        nudPort.Enabled = !running;
        btnBrowse.Enabled = !running;

        lblStatus.Text = running ? "● ĐANG CHẠY" : "● ĐÃ DỪNG";
        lblStatus.ForeColor = running
            ? Color.FromArgb(21, 128, 61)
            : Color.FromArgb(185, 28, 28);
    }

    private void Ui(Action action)
    {
        if (IsDisposed)
            return;

        if (InvokeRequired)
            BeginInvoke(action);
        else
            action();
    }
}
