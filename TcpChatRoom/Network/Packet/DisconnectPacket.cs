namespace TcpChatRoom.Network.Packet;

public class DisconnectPacket : Packet
{
    public override PacketType PacketType => PacketType.Disconnect;

    public override int RawLength =>
        base.RawLength + 
        StreamUtils.LengthOf(Reason);

    public string? Reason { get; set; }

    public override void ReadFrom(Stream stream)
    {
        base.ReadFrom(stream);
        Reason = StreamUtils.ReadNullableString(stream);
    }

    public override void WriteTo(Stream stream)
    {
        base.WriteTo(stream);
        StreamUtils.Write(stream, Reason);
    }
}
