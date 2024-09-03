namespace TcpChatRoom.Network.Packet;

public class HandshakePacket : Packet
{
    public override PacketType PacketType => PacketType.Handshake;

    public override int RawLength =>
        base.RawLength +
        StreamUtils.LengthOf(Version) +
        StreamUtils.LengthOf(Name) +
        StreamUtils.LengthOf(HelloMessage) +
        StreamUtils.LengthOf(Authentication);

    public long Version { get; set; }
    public string? Name { get; set; }
    public string? HelloMessage { get; set; }
    public string? Authentication { get; set; }

    public override void ReadFrom(Stream stream)
    {
        base.ReadFrom(stream);
        Version = StreamUtils.ReadI64(stream);
        Name = StreamUtils.ReadNullableString(stream);
        HelloMessage = StreamUtils.ReadNullableString(stream);
        Authentication = StreamUtils.ReadNullableString(stream);
    }

    public override void WriteTo(Stream stream)
    {
        base.WriteTo(stream);
        StreamUtils.Write(stream, Version);
        StreamUtils.Write(stream, Name);
        StreamUtils.Write(stream, HelloMessage);
        StreamUtils.Write(stream, Authentication);
    }
}
