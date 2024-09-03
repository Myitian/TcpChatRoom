namespace TcpChatRoom.Network.Packet;

public interface ISingleContainerPacket
{
    Packet Body { get; set; }
}
