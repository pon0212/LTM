using System.Drawing;
using System.Windows.Forms;

namespace FileSocketClient;

partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblIp = new Label();
        txtIp = new TextBox();
        lblPort = new Label();
        nudPort = new NumericUpDown();
        lblFileName = new Label();
        txtFileName = new TextBox();
        lblSavePath = new Label();
        txtSavePath = new TextBox();
        btnBrowse = new Button();
        btnDownload = new Button();
        btnCancel = new Button();
        progressBar = new ProgressBar();
        rtbLog = new RichTextBox();

        ((System.ComponentModel.ISupportInitialize)nudPort).BeginInit();
        SuspendLayout();

        // lblIp
        lblIp.AutoSize = true;
        lblIp.Location = new Point(24, 25);
        lblIp.Name = "lblIp";
        lblIp.Size = new Size(125, 20);
        lblIp.Text = "Địa chỉ IP Server:";

        // txtIp
        txtIp.Location = new Point(155, 22);
        txtIp.Name = "txtIp";
        txtIp.Size = new Size(220, 27);
        txtIp.Text = "127.0.0.1";

        // lblPort
        lblPort.AutoSize = true;
        lblPort.Location = new Point(405, 25);
        lblPort.Name = "lblPort";
        lblPort.Size = new Size(39, 20);
        lblPort.Text = "Port:";

        // nudPort
        nudPort.Location = new Point(450, 22);
        nudPort.Maximum = 65535;
        nudPort.Minimum = 1;
        nudPort.Name = "nudPort";
        nudPort.Size = new Size(110, 27);
        nudPort.Value = 8888;

        // lblFileName
        lblFileName.AutoSize = true;
        lblFileName.Location = new Point(24, 70);
        lblFileName.Name = "lblFileName";
        lblFileName.Size = new Size(128, 20);
        lblFileName.Text = "Tên file trên Server:";

        // txtFileName
        txtFileName.Anchor =
            AnchorStyles.Top |
            AnchorStyles.Left |
            AnchorStyles.Right;
        txtFileName.Location = new Point(155, 67);
        txtFileName.Name = "txtFileName";
        txtFileName.Size = new Size(710, 27);
        txtFileName.Text = "Welcome.txt";

        // lblSavePath
        lblSavePath.AutoSize = true;
        lblSavePath.Location = new Point(24, 115);
        lblSavePath.Name = "lblSavePath";
        lblSavePath.Size = new Size(125, 20);
        lblSavePath.Text = "Nơi lưu trên Client:";

        // txtSavePath
        txtSavePath.Anchor =
            AnchorStyles.Top |
            AnchorStyles.Left |
            AnchorStyles.Right;
        txtSavePath.Location = new Point(155, 112);
        txtSavePath.Name = "txtSavePath";
        txtSavePath.Size = new Size(600, 27);

        // btnBrowse
        btnBrowse.Anchor =
            AnchorStyles.Top |
            AnchorStyles.Right;
        btnBrowse.Location = new Point(770, 110);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Size = new Size(95, 30);
        btnBrowse.Text = "Chọn...";
        btnBrowse.UseVisualStyleBackColor = true;
        btnBrowse.Click += btnBrowse_Click;

        // btnDownload
        btnDownload.Location = new Point(155, 157);
        btnDownload.Name = "btnDownload";
        btnDownload.Size = new Size(120, 36);
        btnDownload.Text = "Tải file";
        btnDownload.UseVisualStyleBackColor = true;
        btnDownload.Click += btnDownload_Click;

        // btnCancel
        btnCancel.Enabled = false;
        btnCancel.Location = new Point(285, 157);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(120, 36);
        btnCancel.Text = "Hủy";
        btnCancel.UseVisualStyleBackColor = true;
        btnCancel.Click += btnCancel_Click;

        // progressBar
        progressBar.Anchor =
            AnchorStyles.Top |
            AnchorStyles.Left |
            AnchorStyles.Right;
        progressBar.Location = new Point(420, 164);
        progressBar.Maximum = 100;
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(445, 23);
        progressBar.Style = ProgressBarStyle.Continuous;

        // rtbLog
        rtbLog.Anchor =
            AnchorStyles.Top |
            AnchorStyles.Bottom |
            AnchorStyles.Left |
            AnchorStyles.Right;
        rtbLog.BackColor = SystemColors.Window;
        rtbLog.Font = new Font("Consolas", 10F);
        rtbLog.Location = new Point(24, 215);
        rtbLog.Name = "rtbLog";
        rtbLog.ReadOnly = true;
        rtbLog.Size = new Size(841, 330);
        rtbLog.WordWrap = false;

        // MainForm
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(889, 570);

        Controls.Add(rtbLog);
        Controls.Add(progressBar);
        Controls.Add(btnCancel);
        Controls.Add(btnDownload);
        Controls.Add(btnBrowse);
        Controls.Add(txtSavePath);
        Controls.Add(lblSavePath);
        Controls.Add(txtFileName);
        Controls.Add(lblFileName);
        Controls.Add(nudPort);
        Controls.Add(lblPort);
        Controls.Add(txtIp);
        Controls.Add(lblIp);

        MinimumSize = new Size(760, 500);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "LAB 6 - TCP File Client";

        FormClosing += MainForm_FormClosing;

        ((System.ComponentModel.ISupportInitialize)nudPort).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    private Label lblIp = null!;
    private TextBox txtIp = null!;
    private Label lblPort = null!;
    private NumericUpDown nudPort = null!;
    private Label lblFileName = null!;
    private TextBox txtFileName = null!;
    private Label lblSavePath = null!;
    private TextBox txtSavePath = null!;
    private Button btnBrowse = null!;
    private Button btnDownload = null!;
    private Button btnCancel = null!;
    private ProgressBar progressBar = null!;
    private RichTextBox rtbLog = null!;
}