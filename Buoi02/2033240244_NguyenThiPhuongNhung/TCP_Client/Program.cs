
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCP_Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("CLIENT START");
            try
            {
                Socket sever = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sever.Connect(new IPEndPoint(IPAddress.Loopback, 9090));
                Console.WriteLine("Da ket noi den Sever. Dang gui du lieu");

                sever.Send(Encoding.ASCII.GetBytes("Hello 1"));
                sever.Send(Encoding.ASCII.GetBytes("Hello 2"));
                sever.Send(Encoding.ASCII.GetBytes("Hello 3"));
                sever.Send(Encoding.ASCII.GetBytes("Hello 4"));
                sever.Send(Encoding.ASCII.GetBytes("Hello 5"));

                Console.WriteLine("Data send. Dong ket noi");

                // Ngắt kết nối để ép TCP flush dữ liệu và gửi cờ FIN
                sever.Shutdown(SocketShutdown.Both);
                sever.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.ReadLine();
        }
    }
}
