namespace TcpChatRoom.Network.Packet;

public class RawPacket(PacketType type, int length) : Packet
{
    public override PacketType PacketType { get; } = type;

    public override int RawLength =>
        base.RawLength +
        length;
    public MemoryBuffer<byte> Payload { get; } = new(length);

    public override void ReadFrom(Stream stream)
    {
        base.ReadFrom(stream);
        stream.ReadExactly(Payload);
    }

    public override void WriteTo(Stream stream)
    {
        base.WriteTo(stream);
        stream.Write(Payload.Span);
    }
}
