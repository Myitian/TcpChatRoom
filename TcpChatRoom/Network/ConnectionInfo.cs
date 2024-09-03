using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using TcpChatRoom.Network.Packet;

namespace TcpChatRoom.Network
{
    public record ConnectionInfo(TcpClient Client, int ID = 0)
    {
        public BlockingCollection<Packet.Packet> SendingPacketQueue { get; } = [];
        public NetworkStream NetworkStream { get; } = Client.GetStream();
        public string StringIP { get; } = Client.Client.RemoteEndPoint is IPEndPoint ipep ? ipep.Address.ToString() : "未知";
    }
}
