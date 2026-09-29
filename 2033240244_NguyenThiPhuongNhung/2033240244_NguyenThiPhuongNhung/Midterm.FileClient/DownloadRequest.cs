public class DownloadRequest
{
    public string Host { get; set; } = "";
    public int Port { get; set; }

    public string RemoteFile { get; set; } = "";
    public string LocalFile { get; set; } = "";
}