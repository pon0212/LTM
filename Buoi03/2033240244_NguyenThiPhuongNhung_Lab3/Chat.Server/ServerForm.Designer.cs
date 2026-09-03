namespace Chat.Server;

partial class ServerForm
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
        lblIp = new Label();
        txtIpAddress = new TextBox();
        lblPort = new Label();
        nudPort = new NumericUpDown();
        btnStart = new Button();
        btnStop = new Button();
        lblStatusTitle = new Label();
        lblStatus = new Label();
        grpLog = new GroupBox();
        rtbLog = new RichTextBox();
        grpClients = new GroupBox();
        lstClients = new ListBox();

        ((System.ComponentModel.ISupportInitialize)nudPort).BeginInit();

        grpLog.SuspendLayout();
        grpClients.SuspendLayout();

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
        lblTitle.Size = new Size(289, 32);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "TCP MULTITHREAD SERVER";

        // =========================
        // lblIp
        // =========================
        lblIp.AutoSize = true;
        lblIp.Font = new Font(
            "Segoe UI",
            10F,
            FontStyle.Bold);

        lblIp.Location = new Point(28, 78);
        lblIp.Name = "lblIp";
        lblIp.Size = new Size(82, 19);
        lblIp.TabIndex = 1;
        lblIp.Text = "Địa chỉ IP:";

        // =========================
        // txtIpAddress
        // =========================
        txtIpAddress.Font = new Font(
            "Segoe UI",
            10F);

        txtIpAddress.Location = new Point(116, 74);
        txtIpAddress.Name = "txtIpAddress";
        txtIpAddress.Size = new Size(180, 25);
        txtIpAddress.TabIndex = 2;
        txtIpAddress.Text = "0.0.0.0";

        // =========================
        // lblPort
        // =========================
        lblPort.AutoSize = true;
        lblPort.Font = new Font(
            "Segoe UI",
            10F,
            FontStyle.Bold);

        lblPort.Location = new Point(320, 78);
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

        nudPort.Location = new Point(365, 74);
        nudPort.Maximum = 65535;
        nudPort.Minimum = 1;
        nudPort.Name = "nudPort";
        nudPort.Size = new Size(100, 25);
        nudPort.TabIndex = 4;
        nudPort.Value = 9000;

        // =========================
        // btnStart
        // =========================
        btnStart.Font = new Font(
            "Segoe UI",
            10F,
            FontStyle.Bold);

        btnStart.Location = new Point(500, 69);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(115, 36);
        btnStart.TabIndex = 5;
        btnStart.Text = "Bắt đầu";
        btnStart.UseVisualStyleBackColor = true;

        btnStart.Click += BtnStart_Click;

        // =========================
        // btnStop
        // =========================
        btnStop.Enabled = false;

        btnStop.Font = new Font(
            "Segoe UI",
            10F,
            FontStyle.Bold);

        btnStop.Location = new Point(625, 69);
        btnStop.Name = "btnStop";
        btnStop.Size = new Size(115, 36);
        btnStop.TabIndex = 6;
        btnStop.Text = "Dừng";
        btnStop.UseVisualStyleBackColor = true;

        btnStop.Click += BtnStop_Click;

        // =========================
        // lblStatusTitle
        // =========================
        lblStatusTitle.AutoSize = true;

        lblStatusTitle.Font = new Font(
            "Segoe UI",
            10F,
            FontStyle.Bold);

        lblStatusTitle.Location = new Point(28, 125);
        lblStatusTitle.Name = "lblStatusTitle";
        lblStatusTitle.Size = new Size(82, 19);
        lblStatusTitle.TabIndex = 7;
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

        lblStatus.Location = new Point(116, 125);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(77, 19);
        lblStatus.TabIndex = 8;
        lblStatus.Text = "ĐÃ DỪNG";

        // =========================
        // grpLog
        // =========================
        grpLog.Controls.Add(rtbLog);

        grpLog.Font = new Font(
            "Segoe UI",
            10F,
            FontStyle.Bold);

        grpLog.Location = new Point(25, 165);
        grpLog.Name = "grpLog";
        grpLog.Size = new Size(535, 390);
        grpLog.TabIndex = 9;
        grpLog.TabStop = false;
        grpLog.Text = "NHẬT KÝ SERVER";

        // =========================
        // rtbLog
        // =========================
        rtbLog.BackColor = Color.White;
        rtbLog.Dock = DockStyle.Fill;

        rtbLog.Font = new Font(
            "Consolas",
            10F);

        rtbLog.Location = new Point(3, 21);
        rtbLog.Name = "rtbLog";
        rtbLog.ReadOnly = true;
        rtbLog.Size = new Size(529, 366);
        rtbLog.TabIndex = 0;
        rtbLog.Text = "";

        // =========================
        // grpClients
        // =========================
        grpClients.Controls.Add(lstClients);

        grpClients.Font = new Font(
            "Segoe UI",
            10F,
            FontStyle.Bold);

        grpClients.Location = new Point(575, 165);
        grpClients.Name = "grpClients";
        grpClients.Size = new Size(250, 390);
        grpClients.TabIndex = 10;
        grpClients.TabStop = false;
        grpClients.Text = "CLIENT ĐANG KẾT NỐI (0)";

        // =========================
        // lstClients
        // =========================
        lstClients.Dock = DockStyle.Fill;

        lstClients.Font = new Font(
            "Segoe UI",
            10F);

        lstClients.FormattingEnabled = true;
        lstClients.ItemHeight = 17;
        lstClients.Location = new Point(3, 21);
        lstClients.Name = "lstClients";
        lstClients.Size = new Size(244, 366);
        lstClients.TabIndex = 0;

        // =========================
        // ServerForm
        // =========================
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;

        BackColor = Color.FromArgb(
            241,
            245,
            249);

        ClientSize = new Size(850, 580);

        Controls.Add(grpClients);
        Controls.Add(grpLog);
        Controls.Add(lblStatus);
        Controls.Add(lblStatusTitle);
        Controls.Add(btnStop);
        Controls.Add(btnStart);
        Controls.Add(nudPort);
        Controls.Add(lblPort);
        Controls.Add(txtIpAddress);
        Controls.Add(lblIp);
        Controls.Add(lblTitle);

        MinimumSize = new Size(866, 619);

        Name = "ServerForm";

        StartPosition = FormStartPosition.CenterScreen;

        Text = "TCP Multithread Chat Server";

        FormClosing += ServerForm_FormClosing;

        ((System.ComponentModel.ISupportInitialize)nudPort).EndInit();

        grpLog.ResumeLayout(false);
        grpClients.ResumeLayout(false);

        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblTitle;
    private Label lblIp;
    private TextBox txtIpAddress;
    private Label lblPort;
    private NumericUpDown nudPort;

    private Button btnStart;
    private Button btnStop;

    private Label lblStatusTitle;
    private Label lblStatus;

    private GroupBox grpLog;
    private RichTextBox rtbLog;

    private GroupBox grpClients;
    private ListBox lstClients;
}