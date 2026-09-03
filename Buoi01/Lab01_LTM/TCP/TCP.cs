
// Khai bao 
using System;
using System.Net;
using System.Net.Sockets;

namespace TCP
{
    class TCP
    {
        static void Main(string[] args)
        {
            Socket severS = null;
            try
            {
                // 1. Tạo EndPoint
                IPEndPoint severIEP = new IPEndPoint(IPAddress.Any, 5000);

                // 2. Tạo Socket
                severS = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // 3. Gắn Socket ở EndPoint
                severS.Bind(severIEP);

                // 4. Bắt đầu lắng nghe
                severS.Listen(10);
                Console.WriteLine("Sever dang lang nghe port 5000");

                // 5. Chấp nhận kết nối
                Socket clientS = severS.Accept();

                // 6. Hiển thị thông tin
                IPEndPoint clientIEP = (IPEndPoint)clientS.RemoteEndPoint;
                Console.WriteLine($"[+] Client moi ket noi");
                Console.WriteLine($" - IP Address: {clientIEP.Address}");
                Console.WriteLine($" - Port: {clientIEP.Port}");

                // Đóng kết nối
                clientS.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[-] Lỗi hệ thống: {ex.Message}");
            }
            finally
            {
                // Đảm bảo server luôn được dọn dẹp 
                if (severS != null)
                {
                    severS.Close();
                }
                Console.WriteLine("Sever da dong!");
            }
            Console.ReadLine();
        }
    }
}
