using System;
using System.Net;
using System.Text;
using System.Net.Sockets;


namespace Client
{
    class Client
    {
        static void Main(string[] args)
        {
            Socket clientS = null;
            try
            {
                // 1. Xác định đích đến 
                IPEndPoint severIEP = new IPEndPoint(IPAddress.Parse("172.0.0.1"), 5000);

                // 2. Khởi tạo Socket với giao thức TCP
                clientS = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Console.WriteLine("Dang ket noi den Sever");

                // 3. Mở kết nối
                clientS.Connect(severIEP);
                Console.WriteLine("Ket noi thanh cong");

                // 4. Chuẩn bị thông điệp và mã hóa thành Bytes
                string message = "Canh bao: He thong da bi xam nhap";
                byte[] dataPayload = Encoding.UTF8.GetBytes(message);

                // 5. Đẩy dữ liệu qua luồng mạng
                int byteSent = clientS.Send(dataPayload);
                Console.WriteLine($"[>] Da gui {byteSent} bytes den Sever");
            }
            catch (SocketException ex) 
            {
                // Lỗi này xảy ra khi sever chưa chạy hoặc bị tường lửa chặn port 5000
                Console.WriteLine($"[-] Tu choi ket noi: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[-] Loi he thong: {ex.Message}");
            }
            finally
            {
                if ( clientS != null && clientS.Connected) 
                {
                    clientS.Shutdown(SocketShutdown.Both);
                    clientS.Close();
                }
            }
            Console.ReadLine();
        }
    }
}
