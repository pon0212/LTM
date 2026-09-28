namespace FileTransfer.Shared;

public sealed class DownloadResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public long FileSize { get; init; }
}