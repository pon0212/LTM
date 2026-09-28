namespace FileTransfer.Shared;

public sealed class DownloadRequest
{
    public string Command { get; init; } = "DOWNLOAD";
    public string FileName { get; init; } = string.Empty;
}