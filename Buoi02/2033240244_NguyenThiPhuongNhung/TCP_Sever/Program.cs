
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCP_Sever
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SEVER START");

            Socket Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Listener.Bind(new IPEndPoint(IPAddress.Any, 9090));
            Listener.Listen(10);
            Console.WriteLine("Cho ket noi....");

            Socket client = Listener.Accept();
            Console.WriteLine($"Client da ket noi {client.RemoteEndPoint}");

            // Tạo bộ đệm nhận dữ liệu
            byte[] buffer = new byte[1024];
            int recv;

            // Vòng lập nhận dữ liệu
            for (int i = 0; i < 5; i++)
            {
                recv = client.Receive(buffer);
                
                // Nếu Client đã đóng kết nối, Receive trả về 0
                if (recv == 0)
                {
                    Console.WriteLine($"Vong lap {i} ket noi duoc dong boi CLient");
                    break;
                }
                Console.WriteLine($"Vong lap {i} da nhan: " + $"{Encoding.ASCII.GetString(buffer,0,recv)}");
            }
            client.Close();
            Listener.Close();
            Console.ReadLine();
        }
    }
}
