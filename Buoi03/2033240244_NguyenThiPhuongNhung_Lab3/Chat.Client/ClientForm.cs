using Chat.Client.Networking;
using Chat.Shared.Protocol;

namespace Chat.Client;

public partial class ClientForm : Form
{
    private readonly ChatClient _client = new();

    public ClientForm()
    {
        InitializeComponent();

        _client.MessageReceived += message =>
            RunOnUi(() => ShowMessage(message));

        _client.UserListReceived += users =>
            RunOnUi(() => ShowUsers(users));

        _client.ErrorOccurred += error =>
            RunOnUi(() =>
                AppendMessage(
                    $"[LỖI] {error}"));

        _client.Disconnected += reason =>
            RunOnUi(() =>
            {
                AppendMessage(
                    $"[HỆ THỐNG] {reason}");

                SetConnectedState(false);
            });

        SetConnectedState(false);
    }

    private void btnConnect_Click(
        object sender,
        EventArgs e)
    {
        string host =
            txtServerIp.Text.Trim();

        string username =
            txtUsername.Text.Trim();

        int port =
            decimal.ToInt32(
                nudPort.Value);

        btnConnect.Enabled = false;

        lblStatus.Text =
            "ĐANG KẾT NỐI...";

        lblStatus.ForeColor =
            Color.DarkOrange;

        var connectThread =
            new Thread(() =>
            {
                try
                {
                    _client.Connect(
                        host,
                        port,
                        username);

                    RunOnUi(() =>
                    {
                        SetConnectedState(true);

                        AppendMessage(
                            $"[HỆ THỐNG] Đã kết nối đến {host}:{port}.");

                        txtMessage.Focus();
                    });
                }
                catch (Exception ex)
                {
                    RunOnUi(() =>
                    {
                        SetConnectedState(false);

                        MessageBox.Show(
                            ex.Message,
                            "Kết nối thất bại",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    });
                }
            })
            {
                IsBackground = true,
                Name =
                    "ChatClient-ConnectThread"
            };

        connectThread.Start();
    }

    private void btnDisconnect_Click(
        object sender,
        EventArgs e)
    {
        _client.Disconnect();

        SetConnectedState(false);

        AppendMessage(
            "[HỆ THỐNG] Đã ngắt kết nối.");
    }

    private void btnSend_Click(
        object sender,
        EventArgs e)
    {
        SendCurrentMessage();
    }

    private void txtMessage_KeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;

            SendCurrentMessage();
        }
    }

    private void SendCurrentMessage()
    {
        try
        {
            string content =
                txtMessage.Text;

            _client.SendChat(content);

            txtMessage.Clear();
            txtMessage.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Không thể gửi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void ShowMessage(
        ChatMessage message)
    {
        switch (message.Type)
        {
            case MessageType.Chat:
                AppendMessage(
                    $"[{message.SentAt.LocalDateTime:HH:mm:ss}] " +
                    $"{message.Sender}: {message.Content}");
                break;

            case MessageType.System:
                AppendMessage(
                    $"[HỆ THỐNG] {message.Content}");
                break;

            case MessageType.Error:
                AppendMessage(
                    $"[LỖI] {message.Content}");
                break;
        }
    }

    private void AppendMessage(
        string message)
    {
        rtbMessages.AppendText(
            message +
            Environment.NewLine);

        rtbMessages.ScrollToCaret();
    }

    private void ShowUsers(
        IReadOnlyList<string> users)
    {
        lstUsers.BeginUpdate();

        try
        {
            lstUsers.Items.Clear();

            foreach (string user in users)
            {
                lstUsers.Items.Add(user);
            }

            grpUsers.Text =
                $"TRỰC TUYẾN ({users.Count})";
        }
        finally
        {
            lstUsers.EndUpdate();
        }
    }

    private void SetConnectedState(
        bool connected)
    {
        btnConnect.Enabled =
            !connected;

        btnDisconnect.Enabled =
            connected;

        txtServerIp.Enabled =
            !connected;

        nudPort.Enabled =
            !connected;

        txtUsername.Enabled =
            !connected;

        txtMessage.Enabled =
            connected;

        btnSend.Enabled =
            connected;

        if (connected)
        {
            lblStatus.Text =
                "ĐÃ KẾT NỐI";

            lblStatus.ForeColor =
                Color.Green;
        }
        else
        {
            lblStatus.Text =
                "CHƯA KẾT NỐI";

            lblStatus.ForeColor =
                Color.Red;

            lstUsers.Items.Clear();

            grpUsers.Text =
                "TRỰC TUYẾN (0)";
        }
    }

    private void RunOnUi(
        Action action)
    {
        if (IsDisposed ||
            !IsHandleCreated)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(action);
        }
        else
        {
            action();
        }
    }

    private void ClientForm_FormClosing(
        object sender,
        FormClosingEventArgs e)
    {
        _client.Dispose();
    }
}