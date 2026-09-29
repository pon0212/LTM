using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

class Program
{
    private static readonly object _resultLock = new object();

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("===== FILE CLIENT =====");
        Console.WriteLine();

        string requestFile = Path.Combine(AppContext.BaseDirectory, "requests.txt");

        if (!File.Exists(requestFile))
        {
            CreateSampleRequestFile(requestFile);

            Console.WriteLine(
                $"Da tao file mau: {requestFile}"
            );

            Console.WriteLine(
                "Hay kiem tra noi dung file requests.txt roi chay lai chuong trinh."
            );

            return;
        }

        List<DownloadRequest> requests =
            LoadRequests(requestFile);

        if (requests.Count == 0)
        {
            Console.WriteLine(
                "Khong co yeu cau tai file hop le."
            );

            return;
        }

        Console.WriteLine(
            $"Tong so yeu cau: {requests.Count}"
        );

        Console.Write(
            "So luot tai dong thoi toi da (Enter = 4): "
        );

        string? maxText =
            Console.ReadLine();

        int maxConcurrent = 4;

        if (!string.IsNullOrWhiteSpace(maxText))
        {
            if (!int.TryParse(
                maxText,
                out maxConcurrent) ||
                maxConcurrent <= 0)
            {
                Console.WriteLine(
                    "Gia tri khong hop le. Su dung mac dinh = 4."
                );

                maxConcurrent = 4;
            }
        }

        Console.WriteLine();
        Console.WriteLine(
            $"Bat dau tai voi toi da {maxConcurrent} luot dong thoi..."
        );

        Console.WriteLine();

        SemaphoreSlim downloadLimit =
            new SemaphoreSlim(
                maxConcurrent,
                maxConcurrent
            );

        List<DownloadResult> results =
            new List<DownloadResult>();

        CountdownEvent countdown =
            new CountdownEvent(
                requests.Count
            );

        FileDownloader downloader =
            new FileDownloader();

        foreach (DownloadRequest request in requests)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                downloadLimit.Wait();

                try
                {
                    DownloadResult result =
                        downloader.DownloadFile(request);

                    lock (_resultLock)
                    {
                        results.Add(result);
                    }
                }
                catch (Exception ex)
                {
                    DownloadResult result =
                        new DownloadResult
                        {
                            Server =
                                $"{request.Host}:{request.Port}",

                            RemoteFile =
                                request.RemoteFile,

                            LocalFile =
                                request.LocalFile,

                            Status =
                                "ERROR",

                            Size = 0,

                            TimeMs = 0,

                            AverageSpeed = 0
                        };

                    lock (_resultLock)
                    {
                        results.Add(result);

                        Console.WriteLine(
                            $"Loi tai {request.RemoteFile}: {ex.Message}"
                        );
                    }
                }
                finally
                {
                    downloadLimit.Release();

                    countdown.Signal();
                }
            });
        }

        countdown.Wait();

        Console.WriteLine();
        Console.WriteLine(
            "===== KET QUA ====="
        );

        PrintResults(results);

        string reportFile = Path.Combine(AppContext.BaseDirectory, "download-report.csv");

        ExportCsv(
            reportFile,
            results
        );

        Console.WriteLine();
        Console.WriteLine(
            "Da tao file download-report.csv"
        );
    }

    private static List<DownloadRequest> LoadRequests(
        string filePath)
    {
        List<DownloadRequest> requests =
            new List<DownloadRequest>();

        string[] lines =
            File.ReadAllLines(filePath);

        foreach (string rawLine in lines)
        {
            string line =
                rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("#"))
                continue;

            DownloadRequest? request =
                ParseRequestLine(line);

            if (request != null)
            {
                requests.Add(request);
            }
            else
            {
                Console.WriteLine(
                    $"Bo qua dong khong hop le: {line}"
                );
            }
        }

        return requests;
    }

    private static DownloadRequest? ParseRequestLine(
        string line)
    {
        int firstSpace =
            line.IndexOf(' ');

        if (firstSpace <= 0)
            return null;

        int secondSpace =
            line.IndexOf(
                ' ',
                firstSpace + 1
            );

        if (secondSpace <= firstSpace)
            return null;

        string serverPart =
            line.Substring(
                0,
                firstSpace
            ).Trim();

        string remoteFile =
            line.Substring(
                firstSpace + 1,
                secondSpace - firstSpace - 1
            ).Trim();

        string localFile =
            line.Substring(
                secondSpace + 1
            ).Trim();

        if (string.IsNullOrWhiteSpace(remoteFile) ||
            string.IsNullOrWhiteSpace(localFile))
        {
            return null;
        }

        if (!TryParseHostPort(
            serverPart,
            out string host,
            out int port))
        {
            return null;
        }

        return new DownloadRequest
        {
            Host = host,
            Port = port,
            RemoteFile = remoteFile,
            LocalFile = localFile
        };
    }

    private static bool TryParseHostPort(
        string value,
        out string host,
        out int port)
    {
        host = "";
        port = 0;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        // IPv6 dang [::1]:9100
        if (value.StartsWith("["))
        {
            int endBracket =
                value.IndexOf(']');

            if (endBracket <= 1)
                return false;

            if (endBracket + 1 >= value.Length ||
                value[endBracket + 1] != ':')
            {
                return false;
            }

            host =
                value.Substring(
                    1,
                    endBracket - 1
                );

            string portText =
                value.Substring(
                    endBracket + 2
                );

            if (!int.TryParse(
                portText,
                out port))
            {
                return false;
            }
        }
        else
        {
            int colonIndex =
                value.LastIndexOf(':');

            if (colonIndex <= 0 ||
                colonIndex == value.Length - 1)
            {
                return false;
            }

            host =
                value.Substring(
                    0,
                    colonIndex
                );

            string portText =
                value.Substring(
                    colonIndex + 1
                );

            if (!int.TryParse(
                portText,
                out port))
            {
                return false;
            }
        }

        if (port < 1 ||
            port > 65535)
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(host);
    }

    private static void PrintResults(
        List<DownloadResult> results)
    {
        Console.WriteLine(
            "STT | Server | RemoteFile | LocalFile | Status | Size | TimeMs | AverageSpeed"
        );

        Console.WriteLine(
            new string('-', 120)
        );

        int index = 1;

        foreach (DownloadResult result in results)
        {
            Console.WriteLine(
                $"{index} | " +
                $"{result.Server} | " +
                $"{result.RemoteFile} | " +
                $"{result.LocalFile} | " +
                $"{result.Status} | " +
                $"{result.Size} | " +
                $"{result.TimeMs} | " +
                $"{result.AverageSpeed:F2} MB/s"
            );

            index++;
        }
    }

    private static void ExportCsv(
        string filePath,
        List<DownloadResult> results)
    {
        using StreamWriter writer =
            new StreamWriter(
                filePath,
                false,
                new UTF8Encoding(true)
            );

        writer.WriteLine(
            "STT,Server,RemoteFile,LocalFile,Status,Size,TimeMs,AverageSpeed"
        );

        int index = 1;

        foreach (DownloadResult result in results)
        {
            writer.WriteLine(
                $"{index}," +
                $"{EscapeCsv(result.Server)}," +
                $"{EscapeCsv(result.RemoteFile)}," +
                $"{EscapeCsv(result.LocalFile)}," +
                $"{EscapeCsv(result.Status)}," +
                $"{result.Size}," +
                $"{result.TimeMs}," +
                $"{result.AverageSpeed.ToString("F2", CultureInfo.InvariantCulture)}"
            );

            index++;
        }
    }

    private static string EscapeCsv(
        string value)
    {
        if (value.Contains(',') ||
            value.Contains('"') ||
            value.Contains('\n') ||
            value.Contains('\r'))
        {
            return "\"" +
                value.Replace(
                    "\"",
                    "\"\""
                ) +
                "\"";
        }

        return value;
    }

    private static void CreateSampleRequestFile(
        string filePath)
    {
        string[] lines =
        {
            "# Dinh dang:",
            "# <Host>:<Port> <RemoteFile> <LocalFile>",
            "",
            "localhost:9100 XinChao.txt Downloads/XinChao.txt"
        };

        File.WriteAllLines(
            filePath,
            lines,
            Encoding.UTF8
        );
    }
}