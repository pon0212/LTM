using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("-----------------------------");
            Console.WriteLine("NGUYEN THI PHUONG NHUNG - 2033240244");
            Console.WriteLine("LAB 4 - UDP CLIENT");
            Console.WriteLine("-----------------------------");

            IPEndPoint serverEndPoint =
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);

            Socket server = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp
            );

            Console.WriteLine("Nhap noi dung de gui den Server.");
            Console.WriteLine("Go 'exit' de dong Client.");
            Console.WriteLine("Go 'exit all' de dong ca Client va Server.");
            Console.WriteLine();

            while (true)
            {
                Console.Write("Client > ");
                string message = Console.ReadLine() ?? "";

                byte[] sendBuff = Encoding.ASCII.GetBytes(message);

                server.SendTo(
                    sendBuff,
                    0,
                    sendBuff.Length,
                    SocketFlags.None,
                    serverEndPoint
                );

                if (message == "exit")
                {
                    Console.WriteLine("Client dang dong...");
                    break;
                }

                byte[] receiveBuff = new byte[1024];
                EndPoint remote = new IPEndPoint(IPAddress.Any, 0);

                int byteReceive = server.ReceiveFrom(
                    receiveBuff,
                    0,
                    receiveBuff.Length,
                    SocketFlags.None,
                    ref remote
                );

                string result = Encoding.ASCII.GetString(
                    receiveBuff,
                    0,
                    byteReceive
                );

                Console.WriteLine("Server > " + result);

                if (message == "exit all")
                {
                    Console.WriteLine("Client va Server dang dong...");
                    break;
                }
            }

            server.Close();
        }
    }
}
