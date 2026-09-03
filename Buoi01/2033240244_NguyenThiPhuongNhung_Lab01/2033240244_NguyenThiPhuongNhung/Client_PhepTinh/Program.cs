
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace Client_PhepTinh
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Socket clientS = null;

            try
            {
                // 1. Xác định đích đến   
                IPEndPoint severIEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);

                // 2. Khởi tạo Socket với giao thức TCP
                clientS = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Console.WriteLine("Dang ket noi den Sever");

                // 3. Mở kết nối
                clientS.Connect(severIEP);
                Console.WriteLine("Ket noi thanh cong");

                // 4. Nhập dữ liệu từ Client
                Console.WriteLine("Nhap so thu nhat: ");
                string so1 = Console.ReadLine();

                Console.WriteLine("Nhap so thu hai: ");
                string so2 = Console.ReadLine();

                Console.WriteLine("Nhap phep tinh: ");
                string pheptinh = Console.ReadLine();

                // 5. Thông báo
                string thongbao = so1 + "|" + so2 + "|" + pheptinh;

                // 6. Mã hóa thành Bytes
                byte[] dataPayload = Encoding.UTF8.GetBytes(thongbao);

                // 7. Đẩy dữ liệu
                int byteSend = clientS.Send(dataPayload);
                Console.WriteLine($"Da gui {byteSend} bytes den Sever");

                // 8. Nhận kết quả từ Sever
                byte[] buffer = new byte[1024];

                int byteReceive = clientS.Receive(buffer);
                string ketqua = Encoding.UTF8.GetString(buffer, 0, byteReceive);

                // 9. Hiển thị kết quả
                Console.WriteLine($"Ket qua: {ketqua}");
            }
            catch(SocketException ex)
            {
                // Lỗi này xảy ra khi sever ch chạy hoặc firewall chặn
                Console.WriteLine($"Tu choi ket noi: {ex.Message}");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Loi he thong: {ex.Message}");
            }
            finally
            {
                if(clientS != null && clientS.Connected)
                {
                    clientS.Shutdown(SocketShutdown.Both);
                    clientS.Close();
                }    
            }
            Console.ReadLine();
        }
    }
}
