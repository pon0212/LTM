using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("-----------------------------");
            Console.WriteLine("NGUYEN THI PHUONG NHUNG - 2033240244");
            Console.WriteLine("LAB 4 - UDP SERVER");
            Console.WriteLine("-----------------------------");

            IPEndPoint serverEndPoint =
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);

            Socket serverSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp
            );

            serverSocket.Bind(serverEndPoint);

            Console.WriteLine("UDP Server dang chay tai 127.0.0.1:5000");
            Console.WriteLine("Dang cho Client gui du lieu...");
            Console.WriteLine();

            EndPoint remote = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                byte[] buff = new byte[1024];

                int byteReceive = serverSocket.ReceiveFrom(
                    buff,
                    0,
                    buff.Length,
                    SocketFlags.None,
                    ref remote
                );

                string message = Encoding.ASCII.GetString(
                    buff,
                    0,
                    byteReceive
                );

                Console.WriteLine("Client: " + remote);
                Console.WriteLine("Noi dung: " + message);

                serverSocket.SendTo(
                    buff,
                    0,
                    byteReceive,
                    SocketFlags.None,
                    remote
                );

                if (message == "exit all")
                {
                    Console.WriteLine("Nhan lenh exit all -> Server dang dong...");
                    break;
                }

                Console.WriteLine();
            }

            serverSocket.Close();
        }
    }
}
