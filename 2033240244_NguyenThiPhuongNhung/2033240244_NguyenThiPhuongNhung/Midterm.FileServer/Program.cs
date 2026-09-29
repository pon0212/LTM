using System.Net;

class Program
{
    static void Main(string[] args)
    {
        string ipAddress = args.Length > 0 ? args[0] : "0.0.0.0";
        string portText = args.Length > 1 ? args[1] : "9100";
        string sharedFolder = args.Length > 2
            ? args[2]
            : Path.Combine(AppContext.BaseDirectory, "SharedFiles");
        string maxClientsText = args.Length > 3 ? args[3] : "20";

        if (!IPAddress.TryParse(ipAddress, out IPAddress? ip))
        {
            Console.WriteLine("IP khong hop le.");
            return;
        }

        if (!int.TryParse(portText, out int port) || port < 1 || port > 65535)
        {
            Console.WriteLine("Port khong hop le.");
            return;
        }

        if (!int.TryParse(maxClientsText, out int maxClients) || maxClients <= 0)
        {
            Console.WriteLine("MaxClients khong hop le.");
            return;
        }

        Directory.CreateDirectory(sharedFolder);

        FileServer server = new FileServer(
            ip,
            port,
            Path.GetFullPath(sharedFolder),
            maxClients
        );

        server.Start();
    }
}
