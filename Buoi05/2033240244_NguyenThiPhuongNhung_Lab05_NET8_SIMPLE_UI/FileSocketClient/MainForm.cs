using System.Net.Sockets;
using System.Text;
using FileSocketClient.Services;

namespace FileSocketClient;

public sealed class MainForm : Form
{
    private readonly TextBox txtServerIp = new();
    private readonly NumericUpDown nudPort = new();
    private readonly TextBox txtRemoteFile = new();
    private readonly TextBox txtSavePath = new();
    private readonly Button btnBrowse = new();
    private readonly Button btnDownload = new();
    private readonly Button btnCancel = new();
    private readonly Button btnQuickTest = new();
    private readonly ProgressBar progressBar = new();
    private readonly Label lblPercent = new();
    private readonly Label lblStatus = new();
    private readonly RichTextBox rtbContent = new();

    private readonly FileClientService _service = new();
    private CancellationTokenSource? _cts;

    public MainForm()
    {
        Text = "TCP File Transfer Client - .NET 8";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(820, 590);
        MinimumSize = new Size(760, 540);
        BackColor = Color.FromArgb(245, 247, 250);
        Font = new Font("Segoe UI", 9.5F);
        AutoScaleMode = AutoScaleMode.Dpi;

        ConfigureControls();
        BuildLayout();
        WireEvents();
    }

    private void ConfigureControls()
    {
        txtServerIp.Text = "127.0.0.1";
        txtServerIp.Dock = DockStyle.Fill;

        nudPort.Minimum = 1;
        nudPort.Maximum = 65535;
        nudPort.Value = 8888;
        nudPort.Dock = DockStyle.Fill;

        txtRemoteFile.Text = "Welcome.txt";
        txtRemoteFile.Dock = DockStyle.Fill;

        txtSavePath.Text = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads",
            "Welcome.txt");
        txtSavePath.Dock = DockStyle.Fill;

        btnBrowse.Text = "Chọn nơi lưu";
        btnBrowse.Dock = DockStyle.Fill;

        btnDownload.Text = "Tải file";
        btnDownload.Dock = DockStyle.Fill;
        btnDownload.BackColor = Color.FromArgb(37, 99, 235);
        btnDownload.ForeColor = Color.White;
        btnDownload.FlatStyle = FlatStyle.Flat;

        btnCancel.Text = "Hủy";
        btnCancel.Dock = DockStyle.Fill;
        btnCancel.Enabled = false;
        btnCancel.BackColor = Color.FromArgb(220, 38, 38);
        btnCancel.ForeColor = Color.White;
        btnCancel.FlatStyle = FlatStyle.Flat;

        btnQuickTest.Text = "Test Welcome.txt";
        btnQuickTest.Dock = DockStyle.Fill;
        btnQuickTest.BackColor = Color.FromArgb(226, 232, 240);
        btnQuickTest.FlatStyle = FlatStyle.Flat;

        progressBar.Minimum = 0;
        progressBar.Maximum = 100;
        progressBar.Dock = DockStyle.Fill;

        lblPercent.Text = "0%";
        lblPercent.Dock = DockStyle.Fill;
        lblPercent.TextAlign = ContentAlignment.MiddleCenter;
        lblPercent.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

        lblStatus.Text = "Sẵn sàng.";
        lblStatus.Dock = DockStyle.Fill;
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;

        rtbContent.ReadOnly = true;
        rtbContent.Dock = DockStyle.Fill;
        rtbContent.BackColor = Color.White;
        rtbContent.Font = new Font("Consolas", 9F);
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
            Text = "TCP FILE TRANSFER CLIENT",
            Dock = DockStyle.Top,
            Height = 30,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        var subtitle = new Label
        {
            Text = "Tải file từ Server qua TCP - .NET 8",
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
            RowCount = 3,
            BackColor = Color.FromArgb(245, 247, 250)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 245));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        root.Controls.Add(BuildInputGroup(), 0, 0);
        root.Controls.Add(BuildProgressGroup(), 0, 1);
        root.Controls.Add(BuildPreviewGroup(), 0, 2);

        Controls.Add(root);
        Controls.Add(header);
    }

    private Control BuildInputGroup()
    {
        var group = new GroupBox
        {
            Text = "Thông tin kết nối và file",
            Dock = DockStyle.Fill,
            Padding = new Padding(12)
        };

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 5
        };

        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

        table.Controls.Add(CreateLabel("IP Server:"), 0, 0);
        table.Controls.Add(txtServerIp, 1, 0);
        table.Controls.Add(CreateLabel("Cổng:"), 2, 0);
        table.Controls.Add(nudPort, 3, 0);

        table.Controls.Add(CreateLabel("Tên file:"), 0, 1);
        table.Controls.Add(txtRemoteFile, 1, 1);
        table.SetColumnSpan(txtRemoteFile, 3);

        table.Controls.Add(CreateLabel("Lưu tại:"), 0, 2);
        table.Controls.Add(txtSavePath, 1, 2);
        table.SetColumnSpan(txtSavePath, 2);
        table.Controls.Add(btnBrowse, 3, 2);

        var hint = new Label
        {
            Text = "Ví dụ: Welcome.txt",
            Dock = DockStyle.Fill,
            ForeColor = Color.DimGray,
            TextAlign = ContentAlignment.MiddleLeft
        };
        table.Controls.Add(hint, 1, 3);
        table.SetColumnSpan(hint, 3);

        table.Controls.Add(btnDownload, 1, 4);
        table.Controls.Add(btnCancel, 2, 4);
        table.Controls.Add(btnQuickTest, 3, 4);

        group.Controls.Add(table);
        return group;
    }

    private Control BuildProgressGroup()
    {
        var group = new GroupBox
        {
            Text = "Tiến trình",
            Dock = DockStyle.Fill,
            Padding = new Padding(12)
        };

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2
        };

        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 88F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

        table.Controls.Add(lblStatus, 0, 0);
        table.SetColumnSpan(lblStatus, 2);
        table.Controls.Add(progressBar, 0, 1);
        table.Controls.Add(lblPercent, 1, 1);

        group.Controls.Add(table);
        return group;
    }

    private Control BuildPreviewGroup()
    {
        var group = new GroupBox
        {
            Text = "Nội dung / nhật ký",
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };

        group.Controls.Add(rtbContent);
        return group;
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
            using var dialog = new SaveFileDialog
            {
                FileName = txtRemoteFile.Text.Trim(),
                Filter = "Tất cả file (*.*)|*.*"
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
                txtSavePath.Text = dialog.FileName;
        };

        btnDownload.Click += async (_, _) => await DownloadAsync();
        btnCancel.Click += (_, _) => _cts?.Cancel();

        btnQuickTest.Click += async (_, _) =>
        {
            txtServerIp.Text = "127.0.0.1";
            nudPort.Value = 8888;
            txtRemoteFile.Text = "Welcome.txt";
            txtSavePath.Text = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                "Welcome_Lab5_Test.txt");

            await DownloadAsync();
        };
    }

    private async Task DownloadAsync()
    {
        if (!ValidateInput(out string error))
        {
            MessageBox.Show(
                error,
                "Dữ liệu không hợp lệ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        _cts = new CancellationTokenSource();
        SetBusy(true);

        progressBar.Value = 0;
        lblPercent.Text = "0%";
        lblStatus.Text = "Đang kết nối tới Server...";
        rtbContent.Clear();

        var progress = new Progress<DownloadProgress>(p =>
        {
            int value = (int)Math.Clamp(Math.Round(p.Percent), 0, 100);
            progressBar.Value = value;
            lblPercent.Text = value + "%";
            lblStatus.Text =
                $"Đang tải: {p.ReceivedBytes:N0} / {p.TotalBytes:N0} byte";
        });

        try
        {
            var response = await _service.DownloadAsync(
                txtServerIp.Text.Trim(),
                (int)nudPort.Value,
                txtRemoteFile.Text.Trim(),
                txtSavePath.Text.Trim(),
                progress,
                _cts.Token);

            if (!response.Success)
            {
                lblStatus.Text = "Server từ chối: " + response.Message;

                MessageBox.Show(
                    response.Message,
                    "Server từ chối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            await PreviewTextIfPossibleAsync(txtSavePath.Text.Trim());

            lblStatus.Text = "Hoàn tất: " + txtSavePath.Text.Trim();

            MessageBox.Show(
                $"Tải file thành công!\n\n{txtSavePath.Text.Trim()}\n{response.FileSize} byte",
                "Hoàn tất",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (OperationCanceledException)
        {
            lblStatus.Text = "Đã hủy tải file.";

            MessageBox.Show(
                "Đã hủy tải file.",
                "Đã hủy",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (SocketException ex)
        {
            lblStatus.Text = "Không kết nối được Server.";

            MessageBox.Show(
                "Không kết nối được Server.\n" +
                "Kiểm tra Server đã bấm Khởi động, IP 127.0.0.1 và port 8888.\n\n" +
                ex.Message,
                "Lỗi mạng",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (IOException ex)
        {
            lblStatus.Text = "Lỗi file / dữ liệu truyền.";

            MessageBox.Show(
                "Lỗi file hoặc dữ liệu truyền:\n" + ex.Message,
                "Lỗi I/O",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Có lỗi xảy ra.";

            MessageBox.Show(
                "Lỗi:\n" + ex.Message,
                "Lỗi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            SetBusy(false);
        }
    }

    private bool ValidateInput(out string error)
    {
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(txtServerIp.Text))
        {
            error = "Hãy nhập IP Server.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtRemoteFile.Text))
        {
            error = "Tên file không được rỗng.";
            return false;
        }

        string fileName = txtRemoteFile.Text.Trim();

        if (Path.IsPathRooted(fileName) ||
            fileName.Contains('/') ||
            fileName.Contains('\\') ||
            fileName.Contains("..") ||
            !string.Equals(
                Path.GetFileName(fileName),
                fileName,
                StringComparison.Ordinal))
        {
            error = "Chỉ được nhập tên file, không nhập đường dẫn thư mục.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtSavePath.Text))
        {
            error = "Hãy chọn nơi lưu file.";
            return false;
        }

        return true;
    }

    private async Task PreviewTextIfPossibleAsync(string path)
    {
        FileInfo info = new(path);

        if (info.Length > 1024 * 1024)
        {
            rtbContent.Text = "File quá lớn để xem trước.";
            return;
        }

        string ext = Path.GetExtension(path).ToLowerInvariant();

        string[] textExtensions =
        {
            ".txt", ".csv", ".json", ".xml", ".log", ".md"
        };

        if (!textExtensions.Contains(ext))
        {
            rtbContent.Text =
                "File nhị phân đã tải thành công. Không hiển thị trong ô nội dung.";
            return;
        }

        rtbContent.Text =
            await File.ReadAllTextAsync(path, Encoding.UTF8);
    }

    private void SetBusy(bool busy)
    {
        txtServerIp.Enabled = !busy;
        nudPort.Enabled = !busy;
        txtRemoteFile.Enabled = !busy;
        txtSavePath.Enabled = !busy;
        btnBrowse.Enabled = !busy;
        btnDownload.Enabled = !busy;
        btnQuickTest.Enabled = !busy;
        btnCancel.Enabled = busy;
    }
}
