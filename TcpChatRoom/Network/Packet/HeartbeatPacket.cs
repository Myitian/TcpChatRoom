namespace TcpChatRoom.Network.Packet;

public class HeartbeatPacket : Packet
{
    public override PacketType PacketType => PacketType.Heartbeat;

    public override int RawLength =>
        base.RawLength +
        StreamUtils.LengthOf(ShouldResponse);
    public bool ShouldResponse { get; set; } = false;

    public override void ReadFrom(Stream stream)
    {
        base.ReadFrom(stream);
        ShouldResponse = StreamUtils.ReadBool(stream);
    }

    public override void WriteTo(Stream stream)
    {
        base.WriteTo(stream);
        StreamUtils.Write(stream, ShouldResponse);
    }
}
