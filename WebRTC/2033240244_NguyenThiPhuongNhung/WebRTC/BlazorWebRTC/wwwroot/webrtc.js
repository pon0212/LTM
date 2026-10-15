let localConnection = null;
let dataChannel = null;
let signalingSocket = null;

// Khai báo địa chỉ của Signaling Server (Sử dụng port 5240 của server)
const SIGNALING_URL = "ws://localhost:5240/signal";

window.webRTCInterop = {
    initializeConnection: async function () {
        // 1. Kết nối với Signaling Server qua WebSocket
        signalingSocket = new WebSocket(SIGNALING_URL);

        // 2. Khởi tạo RTCPeerConnection (Kết nối ngang hàng)
        localConnection = new RTCPeerConnection();

        // 3. Khi ICE tìm thấy đường đi, gửi nó cho đối tác qua Server
        localConnection.onicecandidate = (event) => {
            if (event.candidate) {
                signalingSocket.send(JSON.stringify({
                    Type: "candidate",
                    Payload: JSON.stringify(event.candidate)
                }));
            }
        };

        // 4. Khi nhận được luồng Video từ đối tác, hiển thị lên khung remoteVideo
        localConnection.ontrack = (event) => {
            document.getElementById("remoteVideo").srcObject = event.streams[0];
        };

        // 5. Lắng nghe Kênh Chat (Nếu đối tác là người gọi)
        localConnection.ondatachannel = (event) => {
            dataChannel = event.channel;
            dataChannel.onopen = () => console.log("Kênh chat đã mở (Người nhận)");
            dataChannel.onmessage = (e) => console.log("Đối tác gửi: ", e.data);
        };

        // 6. Xử lý các tín hiệu nhận được từ Server (Offer, Answer, Candidate)
        signalingSocket.onmessage = async (event) => {
            const message = JSON.parse(event.data);

            if (message.Type === "offer") {
                await localConnection.setRemoteDescription(new RTCSessionDescription(JSON.parse(message.Payload)));
                const answer = await localConnection.createAnswer();
                await localConnection.setLocalDescription(answer);
                // Gửi Answer trả lại
                signalingSocket.send(JSON.stringify({ Type: "answer", Payload: JSON.stringify(answer) }));
            }
            else if (message.Type === "answer") {
                await localConnection.setRemoteDescription(new RTCSessionDescription(JSON.parse(message.Payload)));
            }
            else if (message.Type === "candidate") {
                await localConnection.addIceCandidate(new RTCIceCandidate(JSON.parse(message.Payload)));
            }
        };

        // 7. Mở Camera/Micro của bản thân hiển thị lên khung localVideo
        const stream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
        document.getElementById("localVideo").srcObject = stream;
        stream.getTracks().forEach(track => localConnection.addTrack(track, stream));
    },

    startCall: async function () {
        // Người bấm gọi sẽ là người chủ động tạo kênh Chat
        dataChannel = localConnection.createDataChannel("chat");
        dataChannel.onopen = () => console.log("Kênh chat đã mở (Người gọi)");
        dataChannel.onmessage = (event) => console.log("Đối tác gửi: ", event.data);

        // Tạo SDP Offer gửi qua Server để mời đối tác kết nối
        const offer = await localConnection.createOffer();
        await localConnection.setLocalDescription(offer);
        signalingSocket.send(JSON.stringify({
            Type: "offer",
            Payload: JSON.stringify(offer)
        }));
    },

    sendMessage: function (message) {
        if (dataChannel && dataChannel.readyState === "open") {
            dataChannel.send(message);
            console.log("Bạn gửi: ", message);
        } else {
            console.error("Kênh chat chưa kết nối!");
        }
    }
};