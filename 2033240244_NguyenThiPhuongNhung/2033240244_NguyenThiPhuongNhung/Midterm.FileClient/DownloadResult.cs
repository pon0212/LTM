public class DownloadResult
{
    public string Server { get; set; } = "";

    public string RemoteFile { get; set; } = "";

    public string LocalFile { get; set; } = "";

    public string Status { get; set; } = "";

    public long Size { get; set; }

    public long TimeMs { get; set; }

    public double AverageSpeed { get; set; }
}