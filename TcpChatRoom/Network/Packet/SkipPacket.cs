namespace TcpChatRoom.Network.Packet;

public class SkipPacket(PacketType type, int length) : Packet
{
    public override PacketType PacketType { get; } = type;

    public override int RawLength { get; } = length;

    public override void ReadFrom(Stream stream)
    {
        base.ReadFrom(stream);
        stream.Skip(RawLength - base.RawLength);
    }

    public override void WriteTo(Stream stream)
    {
        base.WriteTo(stream);
        stream.FillPadding(RawLength - base.RawLength);
    }
}
