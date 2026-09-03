
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace PhepTinh
{
    class Program
    {
        static void Main(string[] args)
        {
            const int PORT = 5000;

            // 1. Tạo EndPoint của Sever
            IPEndPoint severIEP = new IPEndPoint(IPAddress.Any, PORT);

            // 2. Tạo TCP Socket
            Socket severS = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // 3. Gắn Socket vào IP
                severS.Bind(severIEP);
                severS.Listen(10);


                // 4. Cho sever bắt đầu lắng nghe
                Console.WriteLine($"Sever dang lang nghe tại {PORT}");

                while (true)
                {
                    Console.WriteLine("Dang nghe Client ket noi...");

                    // 5. Chấp nhận kết nối
                    Socket clientS = severS.Accept();

                    IPEndPoint clientIEP = clientS.RemoteEndPoint as IPEndPoint;
                    try
                    {
                        // 6. Nhận dữ liệu Client 
                        byte[] buffer = new byte[1024];

                        int byteReceive = clientS.Receive(buffer);

                        string request = Encoding.UTF8.GetString(buffer, 0, byteReceive);
                        Console.WriteLine($"Du lieu nhan: {request}");

                        // 7. Xử lý phép tính
                        string kq = Caculate(request);
                        Console.WriteLine($"Ket qua: {kq}");

                        // 8. Chuyển KQ sang byte
                        byte[] resposneData = Encoding.UTF8.GetBytes(kq);

                        // 9. Gửi kq về Client
                        clientS.Send(resposneData);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Loi xu ly: {ex.Message}");
                    }
                    finally
                    {
                        // 10. Đóng kết nối Client
                        clientS.Shutdown(SocketShutdown.Both);
                        clientS.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Loi Sever: {ex.Message}");
            }
            finally
            {
                severS.Close();
            }
        }
        static string Caculate(string request)
        {
            try
            {
                string[] past = request.Split('|');

                if (past.Length != 3)
                {
                    return "Loi! Du lieu khong hop le";
                }

                double so1 = double.Parse(past[0]);

                double so2 = double.Parse(past[1]);

                string pheptinh = past[2];

                double ketqua;

                switch (pheptinh)
                {
                    case "+":
                        ketqua = so1 + so2;
                        break;

                    case "-":
                        ketqua = so1 - so2;
                        break;

                    case "*":
                        ketqua = so1 * so2;
                        break;

                    case "/":
                        if (so2 == 0)
                        {
                            return "Loi! Khong the chia cho 0";
                        }
                        ketqua = so1 / so2;
                        break;

                    default:
                        return "Loi! Phep toan khong hop le";
                }
                return ketqua.ToString();
            }
            catch
            {
                return "Khong the xu ly";
            }
        }
    }
}


