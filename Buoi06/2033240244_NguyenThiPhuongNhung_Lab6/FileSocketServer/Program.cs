using System.Net.Sockets;
using System.Text;
using FileSocketServer.Services;

namespace FileSocketServer;

internal static class Program
{
    private const int DefaultPort = 8_888;

    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        if (!TryParseArguments(
            args,
            out int port,
            out int throttleMilliseconds,
            out string error))
        {
            Console.WriteLine($"Lỗi tham số: {error}");
            Console.WriteLine("Cú pháp: [port] [--throttle <milliseconds>]");
            return;
        }

        using var shutdownCts = new CancellationTokenSource();

        ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            Console.WriteLine("\nĐã nhận Ctrl+C. Đang dừng Server an toàn...");
            shutdownCts.Cancel();
        };

        Console.CancelKeyPress += cancelHandler;

        try
        {
            var server = new FileServer(port, throttleMilliseconds);
            await server.RunAsync(shutdownCts.Token);
        }
        catch (OperationCanceledException)
            when (shutdownCts.IsCancellationRequested)
        {
        }
        catch (SocketException ex)
        {
            Console.WriteLine(
                $"Không thể khởi động Server. SocketError={ex.SocketErrorCode}. {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Server dừng do lỗi: {ex.Message}");
        }
        finally
        {
            Console.CancelKeyPress -= cancelHandler;
        }
    }

    private static bool TryParseArguments(
        string[] args,
        out int port,
        out int throttleMilliseconds,
        out string error)
    {
        port = DefaultPort;
        throttleMilliseconds = 0;
        error = string.Empty;

        bool portWasSet = false;

        for (int index = 0; index < args.Length; index++)
        {
            string argument = args[index];

            if (string.Equals(
                argument,
                "--throttle",
                StringComparison.OrdinalIgnoreCase))
            {
                if (index + 1 >= args.Length ||
                    !int.TryParse(args[++index], out throttleMilliseconds) ||
                    throttleMilliseconds is < 0 or > 60_000)
                {
                    error = "--throttle phải theo sau bởi số nguyên từ 0 đến 60000.";
                    return false;
                }

                continue;
            }

            if (argument.StartsWith("--", StringComparison.Ordinal))
            {
                error = $"Không nhận diện tham số '{argument}'.";
                return false;
            }

            if (portWasSet ||
                !int.TryParse(argument, out port) ||
                port is < 1 or > 65_535)
            {
                error = "Port phải là số nguyên duy nhất trong khoảng 1..65535.";
                return false;
            }

            portWasSet = true;
        }

        return true;
    }
}