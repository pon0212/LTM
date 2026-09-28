using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileSocketClient.Services;
using FileTransfer.Shared;

namespace FileSocketClient;

public partial class MainForm : Form
{
    private const long TextPreviewLimit = 1_048_576;

    private static readonly HashSet<string> TextExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".txt", ".csv", ".json", ".xml", ".log"
        };

    private readonly FileClientService _fileClientService = new();
    private CancellationTokenSource? _downloadCts;
    private bool _isClosingAfterCancellation;

    private enum DownloadUiState
    {
        Ready,
        Downloading,
        FinishedOrError
    }

    public MainForm()
    {
        InitializeComponent();

        string desktopPath = Environment.GetFolderPath(
            Environment.SpecialFolder.DesktopDirectory);

        if (string.IsNullOrWhiteSpace(desktopPath))
        {
            desktopPath = Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments);
        }

        txtSavePath.Text = Path.Combine(desktopPath, "Welcome.txt");

        ApplyUiState(DownloadUiState.Ready);
        Log("Client sẵn sàng.");
    }

    private void btnBrowse_Click(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Chọn vị trí lưu file",
            FileName = string.IsNullOrWhiteSpace(txtFileName.Text)
                ? "download.bin"
                : Path.GetFileName(txtFileName.Text.Trim()) ?? "download.bin",
            Filter = "Tất cả file (*.*)|*.*",
            OverwritePrompt = true,
            AddExtension = false
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            txtSavePath.Text = dialog.FileName;
        }
    }

    private async void btnDownload_Click(object? sender, EventArgs e)
    {
        if (_downloadCts is not null)
            return;

        if (!TryValidateInput(
            out IPAddress serverAddress,
            out int port,
            out string remoteFileName,
            out string destinationPath))
        {
            return;
        }

        progressBar.Value = 0;
        ApplyUiState(DownloadUiState.Downloading);

        _downloadCts = new CancellationTokenSource();

        var progress = new Progress<double>(UpdateProgress);

        try
        {
            Log($"Đang kết nối {serverAddress}:{port}...");
            Log($"Yêu cầu tải: {remoteFileName}");

            DownloadResponse response =
                await _fileClientService.DownloadFileAsync(
                    serverAddress,
                    port,
                    remoteFileName,
                    destinationPath,
                    progress,
                    _downloadCts.Token);

            if (!response.Success)
            {
                progressBar.Value = 0;
                Log($"Server từ chối: {response.Message}");

                MessageBox.Show(
                    this,
                    response.Message,
                    "Không thể tải file",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            Log($"Hoàn tất: {response.FileSize:N0} byte.");
            Log($"Đã lưu: {destinationPath}");

            await AppendTextPreviewAsync(destinationPath);

            MessageBox.Show(
                this,
                "Tải file hoàn tất.",
                "Thành công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (OperationCanceledException)
        {
            Log("Đã hủy tải file.");
        }
        catch (SocketException ex)
        {
            Log($"Không thể kết nối Server: {ex.Message}");
        }
        catch (EndOfStreamException ex)
        {
            Log(ex.Message);
        }
        catch (IOException ex)
        {
            Log($"Lỗi truyền hoặc lưu file: {ex.Message}");
        }
        catch (Exception ex)
        {
            Log($"Lỗi: {ex.Message}");
        }
        finally
        {
            _downloadCts?.Dispose();
            _downloadCts = null;

            if (!IsDisposed)
                ApplyUiState(DownloadUiState.FinishedOrError);
        }
    }

    private void btnCancel_Click(object? sender, EventArgs e)
    {
        if (_downloadCts is null)
            return;

        btnCancel.Enabled = false;
        _downloadCts.Cancel();
    }

    private async void MainForm_FormClosing(
        object? sender,
        FormClosingEventArgs e)
    {
        if (_downloadCts is null)
            return;

        e.Cancel = true;

        if (_isClosingAfterCancellation)
            return;

        _isClosingAfterCancellation = true;

        _downloadCts.Cancel();

        while (_downloadCts is not null)
        {
            await Task.Delay(50);
        }

        Close();
    }

    private bool TryValidateInput(
        out IPAddress serverAddress,
        out int port,
        out string remoteFileName,
        out string destinationPath)
    {
        serverAddress = IPAddress.None;
        port = (int)nudPort.Value;
        remoteFileName = txtFileName.Text.Trim();
        destinationPath = string.Empty;

        if (!IPAddress.TryParse(
            txtIp.Text.Trim(),
            out IPAddress? parsedAddress))
        {
            MessageBox.Show("Địa chỉ IP không hợp lệ.");
            return false;
        }

        serverAddress = parsedAddress;

        if (string.IsNullOrWhiteSpace(remoteFileName))
        {
            MessageBox.Show("Tên file không được để trống.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtSavePath.Text))
        {
            MessageBox.Show("Phải chọn nơi lưu file.");
            return false;
        }

        destinationPath =
            Path.GetFullPath(txtSavePath.Text.Trim());

        return true;
    }

    private void ApplyUiState(DownloadUiState state)
    {
        bool isDownloading =
            state == DownloadUiState.Downloading;

        txtIp.Enabled = !isDownloading;
        nudPort.Enabled = !isDownloading;
        txtFileName.Enabled = !isDownloading;
        txtSavePath.Enabled = !isDownloading;

        btnBrowse.Enabled = !isDownloading;
        btnDownload.Enabled = !isDownloading;
        btnCancel.Enabled = isDownloading;
    }

    private void UpdateProgress(double percent)
    {
        progressBar.Value =
            (int)Math.Round(
                Math.Clamp(percent, 0, 100));
    }

    private async Task AppendTextPreviewAsync(string filePath)
    {
        var fileInfo = new FileInfo(filePath);

        if (!TextExtensions.Contains(fileInfo.Extension))
            return;

        if (fileInfo.Length >= TextPreviewLimit)
            return;

        string content =
            await File.ReadAllTextAsync(
                filePath,
                Encoding.UTF8);

        rtbLog.AppendText(
            Environment.NewLine +
            "----- XEM TRƯỚC NỘI DUNG -----" +
            Environment.NewLine);

        rtbLog.AppendText(content);

        rtbLog.AppendText(
            Environment.NewLine +
            "----- HẾT NỘI DUNG -----" +
            Environment.NewLine);
    }

    private void Log(string message)
    {
        rtbLog.AppendText(
            $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");

        rtbLog.ScrollToCaret();
    }
}