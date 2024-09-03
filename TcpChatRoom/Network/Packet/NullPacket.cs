namespace TcpChatRoom.Network.Packet;

public class NullPacket : Packet
{
    public override PacketType PacketType => PacketType.Null;
}
