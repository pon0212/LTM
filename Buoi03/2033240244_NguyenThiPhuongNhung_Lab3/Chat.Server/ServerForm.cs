using System.Net;
using Chat.Server.Networking;

namespace Chat.Server;

public partial class ServerForm : Form
{
    private readonly ChatServer _server = new();

    public ServerForm()
    {
        InitializeComponent();

        // Các sự kiện này phát ra từ luồng mạng,
        // không phải UI thread.
        _server.LogReceived += message =>
            RunOnUi(() => AppendLog(message));

        _server.ClientListChanged += users =>
            RunOnUi(() => ShowClients(users));
    }

    private void BtnStart_Click(object? sender, EventArgs e)
    {
        try
        {
            // Kiểm tra địa chỉ IP
            if (!IPAddress.TryParse(
                    txtIpAddress.Text.Trim(),
                    out IPAddress? address))
            {
                throw new FormatException(
                    "Địa chỉ IP không hợp lệ.");
            }

            // Khởi động Server
            _server.Start(
                address,
                decimal.ToInt32(nudPort.Value));

            SetRunningState(isRunning: true);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Không thể khởi động Server",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void BtnStop_Click(object? sender, EventArgs e)
    {
        _server.Stop();

        SetRunningState(isRunning: false);
    }

    private void ServerForm_FormClosing(
        object? sender,
        FormClosingEventArgs e)
    {
        _server.Dispose();
    }

    private void SetRunningState(bool isRunning)
    {
        btnStart.Enabled = !isRunning;
        btnStop.Enabled = isRunning;

        txtIpAddress.Enabled = !isRunning;
        nudPort.Enabled = !isRunning;

        lblStatus.Text =
            isRunning
                ? "ĐANG CHẠY"
                : "ĐÃ DỪNG";

        lblStatus.ForeColor =
            isRunning
                ? Color.SeaGreen
                : Color.Firebrick;
    }

    private void AppendLog(string message)
    {
        rtbLog.AppendText(
            message + Environment.NewLine);

        rtbLog.ScrollToCaret();
    }

    private void ShowClients(
        IReadOnlyList<string> users)
    {
        lstClients.BeginUpdate();

        lstClients.Items.Clear();

        lstClients.Items.AddRange(
            users.Cast<object>().ToArray());

        lstClients.EndUpdate();

        grpClients.Text =
            $"CLIENT ĐANG KẾT NỐI ({users.Count})";
    }

    /// <summary>
    /// WinForms không cho phép cập nhật Control
    /// trực tiếp từ luồng mạng.
    /// BeginInvoke chuyển công việc về UI thread.
    /// </summary>
    private void RunOnUi(Action action)
    {
        if (IsDisposed || Disposing)
        {
            return;
        }

        if (InvokeRequired)
        {
            try
            {
                BeginInvoke(action);
            }
            catch (InvalidOperationException)
            {
            }
        }
        else
        {
            action();
        }
    }
}