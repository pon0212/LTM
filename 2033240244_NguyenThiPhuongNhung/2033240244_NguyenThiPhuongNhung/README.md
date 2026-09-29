# 2033240244_NguyenThiPhuongNhung_KTL1

## Cau truc
- `Midterm.FileServer`: TCP File Server, mac dinh port 9100, ThreadPool, SemaphoreSlim, SHA-256, kiem tra duong dan.
- `Midterm.FileClient`: tai file bang TCP, tep tam `.part`, SHA-256, hien thi tien do, tai dong thoi, xuat CSV.
- `Midterm.FileServer/SharedFiles`: du lieu kiem thu gom `XinChao.txt`, `Test.zip`, `BigFile.bin` (>100 MB).
- `Midterm.FileClient/requests.txt`: danh sach yeu cau tai mau.
- `Midterm.FileClient/download-report.csv`: bao cao ket qua kiem thu tai dong thoi 5 tep tu 2 server.
- `MinhChungKiemThu`: file Word tong hop va cac anh minh chung kiem thu.

## Cach mo va build
1. Giai nen project vao mot thu muc moi.
2. Mo `Midterm.sln` bang Visual Studio 2022.
3. Build bang `Ctrl + Shift + B`.

## Chay Server va Client
### Server mac dinh
Chay project `Midterm.FileServer`.
Neu khong truyen tham so, Server dung:
- Dia chi: `0.0.0.0`
- Port: `9100`
- Thu muc chia se: `SharedFiles`
- MaxClients: `20`

### Client
Chay project `Midterm.FileClient`.
Client doc danh sach tai tu `requests.txt` va cho phep nhap so luot tai dong thoi toi da.

## Mau requests.txt
```text
localhost:9100 XinChao.txt Downloads/S1_XinChao.txt
localhost:9100 Test.zip Downloads/S1_Test.zip
localhost:9100 BigFile.bin Downloads/S1_BigFile.bin
localhost:9200 XinChao.txt Downloads/S2_XinChao.txt
localhost:9200 Test.zip Downloads/S2_Test.zip
```

## Chay Server thu hai de kiem thu 2 server
Co the chay them mot Server o port 9200 voi cung thu muc du lieu kiem thu.
Vi du chay file Server da build voi tham so:
```text
Midterm.FileServer.exe 0.0.0.0 9200 <SharedFolder> 20
```

## Tep ket qua
Sau khi Client chay xong, chuong trinh tao `download-report.csv` trong thu muc lam viec cua Client.
Ban nop kem trong source la bao cao cua lan kiem thu cuoi cung voi 5 tep tu 2 server deu thanh cong.
