namespace TcpChatRoom.Network.Packet;

public class TextMessagePacket : MessagePacket
{
    public override PacketType PacketType => PacketType.TextMessage;

    public override int RawLength =>
        base.RawLength +
        StreamUtils.LengthOf(Text);

    public string Text { get; set; } = "";

    public override void ReadFrom(Stream stream)
    {
        base.ReadFrom(stream);
        Text = StreamUtils.ReadString(stream);
    }

    public override void WriteTo(Stream stream)
    {
        base.WriteTo(stream);
        StreamUtils.Write(stream, Text.AsSpan());
    }
}
