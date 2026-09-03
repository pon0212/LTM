namespace Chat.Client;

partial class ClientForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        lblTitle = new Label();

        lblServerIp = new Label();
        txtServerIp = new TextBox();

        lblPort = new Label();
        nudPort = new NumericUpDown();

        lblUsername = new Label();
        txtUsername = new TextBox();

        btnConnect = new Button();
        btnDisconnect = new Button();

        lblStatusTitle = new Label();
        lblStatus = new Label();

        grpMessages = new GroupBox();
        rtbMessages = new RichTextBox();

        grpUsers = new GroupBox();
        lstUsers = new ListBox();

        txtMessage = new TextBox();
        btnSend = new Button();

        ((System.ComponentModel.ISupportInitialize)nudPort).BeginInit();

        grpMessages.SuspendLayout();
        grpUsers.SuspendLayout();

        SuspendLayout();

        // =========================
        // lblTitle
        // =========================
        lblTitle.AutoSize = true;

        lblTitle.Font = new Font(
            "Segoe UI",
            18F,
            FontStyle.Bold);

        lblTitle.Location = new Point(25, 20);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(272, 32);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "TCP MULTITHREAD CLIENT";

        // =========================
        // lblServerIp
        // =========================
        lblServerIp.AutoSize = true;

        lblServerIp.Font = new Font(
            "Segoe UI",
            10F,
            FontStyle.Bold);

        lblServerIp.Location = new Point(28, 75);
        lblServerIp.Name = "lblServerIp";
        lblServerIp.Size = new Size(75, 19);
        lblServerIp.TabIndex = 1;
        lblServerIp.Text = "Server IP:";

        // =========================
        // txtServerIp
        // =========================
        txtServerIp.Font = new Font(
            "Segoe UI",
            10F);

        txtServerIp.Location = new Point(110, 71);
        txtServerIp.Name = "txtServerIp";
        txtServerIp.Size = new Size(160, 25);
        txtServerIp.TabIndex = 2;
        txtServerIp.Text = "127.0.0.1";

        // =========================
        // lblPort
        // =========================
        lblPort.AutoSize = true;

        lblPort.Font = new Font(
            "Segoe UI",
            10F,
            FontStyle.Bold);

        lblPort.Location = new Point(290, 75);
        lblPort.Name = "lblPort";
        lblPort.Size = new Size(40, 19);
        lblPort.TabIndex = 3;
        lblPort.Text = "Port:";

        // =========================
        // nudPort
        // =========================
        nudPort.Font = new Font(
            "Segoe UI",
            10F);

        nudPort.Location = new Point(335, 71);
        nudPort.Maximum = 65535;
        nudPort.Minimum = 1;
        nudPort.Name = "nudPort";
        nudPort.Size = new Size(95, 25);
        nudPort.TabIndex = 4;
        nudPort.Value = 9000;

        // =========================
        // lblUsername
        // =========================
        lblUsername.AutoSize = true;

        lblUsername.Font = new Font(
            "Segoe UI",
            10F,
            FontStyle.Bold);

        lblUsername.Location = new Point(450, 75);
        lblUsername.Name = "lblUsername";
        lblUsername.Size = new Size(80, 19);
        lblUsername.TabIndex = 5;
        lblUsername.Text = "Username:";

        // =========================
        // txtUsername
        // =========================
        txtUsername.Font = new Font(
            "Segoe UI",
            10F);

        txtUsername.Location = new Point(535, 71);
        txtUsername.MaxLength = 20;
        txtUsername.Name = "txtUsername";
        txtUsername.Size = new Size(160, 25);
        txtUsername.TabIndex = 6;

        // =========================
        // btnConnect
        // =========================
        btnConnect.Font = new Font(
            "Segoe UI",
            10F,
            FontStyle.Bold);

        btnConnect.Location = new Point(710, 65);
        btnConnect.Name = "btnConnect";
        btnConnect.Size = new Size(100, 36);
        btnConnect.TabIndex = 7;
        btnConnect.Text = "Kết nối";
        btnConnect.UseVisualStyleBackColor = true;

        btnConnect.Click += btnConnect_Click;

        // =========================
        // btnDisconnect
        // =========================
        btnDisconnect.Enabled = false;

        btnDisconnect.Font = new Font(
            "Segoe UI",
            10F,
            FontStyle.Bold);

        btnDisconnect.Location = new Point(820, 65);
        btnDisconnect.Name = "btnDisconnect";
        btnDisconnect.Size = new Size(100, 36);
        btnDisconnect.TabIndex = 8;
        btnDisconnect.Text = "Ngắt";
        btnDisconnect.UseVisualStyleBackColor = true;

        btnDisconnect.Click += btnDisconnect_Click;

        // =========================
        // lblStatusTitle
        // =========================
        lblStatusTitle.AutoSize = true;

        lblStatusTitle.Font = new Font(
            "Segoe UI",
            10F,
            FontStyle.Bold);

        lblStatusTitle.Location = new Point(28, 115);
        lblStatusTitle.Name = "lblStatusTitle";
        lblStatusTitle.Size = new Size(82, 19);
        lblStatusTitle.TabIndex = 9;
        lblStatusTitle.Text = "Trạng thái:";

        // =========================
        // lblStatus
        // =========================
        lblStatus.AutoSize = true;

        lblStatus.Font = new Font(
            "Segoe UI",
            10F,
            FontStyle.Bold);

        lblStatus.ForeColor = Color.Firebrick;

        lblStatus.Location = new Point(115, 115);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(117, 19);
        lblStatus.TabIndex = 10;
        lblStatus.Text = "CHƯA KẾT NỐI";

        // =========================
        // grpMessages
        // =========================
        grpMessages.Controls.Add(rtbMessages);

        grpMessages.Font = new Font(
            "Segoe UI",
            10F,
            FontStyle.Bold);

        grpMessages.Location = new Point(25, 155);
        grpMessages.Name = "grpMessages";
        grpMessages.Size = new Size(650, 380);
        grpMessages.TabIndex = 11;
        grpMessages.TabStop = false;
        grpMessages.Text = "TIN NHẮN";

        // =========================
        // rtbMessages
        // =========================
        rtbMessages.BackColor = Color.White;
        rtbMessages.Dock = DockStyle.Fill;

        rtbMessages.Font = new Font(
            "Segoe UI",
            10F);

        rtbMessages.Location = new Point(3, 21);
        rtbMessages.Name = "rtbMessages";
        rtbMessages.ReadOnly = true;
        rtbMessages.Size = new Size(644, 356);
        rtbMessages.TabIndex = 0;
        rtbMessages.Text = "";

        // =========================
        // grpUsers
        // =========================
        grpUsers.Controls.Add(lstUsers);

        grpUsers.Font = new Font(
            "Segoe UI",
            10F,
            FontStyle.Bold);

        grpUsers.Location = new Point(690, 155);
        grpUsers.Name = "grpUsers";
        grpUsers.Size = new Size(230, 380);
        grpUsers.TabIndex = 12;
        grpUsers.TabStop = false;
        grpUsers.Text = "TRỰC TUYẾN (0)";

        // =========================
        // lstUsers
        // =========================
        lstUsers.Dock = DockStyle.Fill;

        lstUsers.Font = new Font(
            "Segoe UI",
            10F);

        lstUsers.FormattingEnabled = true;
        lstUsers.ItemHeight = 17;
        lstUsers.Location = new Point(3, 21);
        lstUsers.Name = "lstUsers";
        lstUsers.Size = new Size(224, 356);
        lstUsers.TabIndex = 0;

        // =========================
        // txtMessage
        // =========================
        txtMessage.Enabled = false;

        txtMessage.Font = new Font(
            "Segoe UI",
            11F);

        txtMessage.Location = new Point(25, 555);
        txtMessage.MaxLength = 1000;
        txtMessage.Name = "txtMessage";
        txtMessage.PlaceholderText =
            "Nhập tin nhắn...";

        txtMessage.Size = new Size(770, 27);
        txtMessage.TabIndex = 13;

        txtMessage.KeyDown +=
            txtMessage_KeyDown;

        // =========================
        // btnSend
        // =========================
        btnSend.Enabled = false;

        btnSend.Font = new Font(
            "Segoe UI",
            10F,
            FontStyle.Bold);

        btnSend.Location = new Point(810, 550);
        btnSend.Name = "btnSend";
        btnSend.Size = new Size(110, 36);
        btnSend.TabIndex = 14;
        btnSend.Text = "Gửi";
        btnSend.UseVisualStyleBackColor = true;

        btnSend.Click += btnSend_Click;

        // =========================
        // ClientForm
        // =========================
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;

        BackColor = Color.FromArgb(
            241,
            245,
            249);

        ClientSize = new Size(950, 610);

        Controls.Add(btnSend);
        Controls.Add(txtMessage);

        Controls.Add(grpUsers);
        Controls.Add(grpMessages);

        Controls.Add(lblStatus);
        Controls.Add(lblStatusTitle);

        Controls.Add(btnDisconnect);
        Controls.Add(btnConnect);

        Controls.Add(txtUsername);
        Controls.Add(lblUsername);

        Controls.Add(nudPort);
        Controls.Add(lblPort);

        Controls.Add(txtServerIp);
        Controls.Add(lblServerIp);

        Controls.Add(lblTitle);

        MinimumSize = new Size(966, 649);

        Name = "ClientForm";

        StartPosition =
            FormStartPosition.CenterScreen;

        Text =
            "TCP Multithread Chat Client";

        FormClosing +=
            ClientForm_FormClosing;

        ((System.ComponentModel.ISupportInitialize)nudPort)
            .EndInit();

        grpMessages.ResumeLayout(false);
        grpUsers.ResumeLayout(false);

        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblTitle;

    private Label lblServerIp;
    private TextBox txtServerIp;

    private Label lblPort;
    private NumericUpDown nudPort;

    private Label lblUsername;
    private TextBox txtUsername;

    private Button btnConnect;
    private Button btnDisconnect;

    private Label lblStatusTitle;
    private Label lblStatus;

    private GroupBox grpMessages;
    private RichTextBox rtbMessages;

    private GroupBox grpUsers;
    private ListBox lstUsers;

    private TextBox txtMessage;
    private Button btnSend;
}