namespace TcpChatRoom.Network.Packet;

public class RichTextMessagePacket : MessagePacket
{
    public override PacketType PacketType => PacketType.RichTextMessage;

    public override int RawLength =>
        base.RawLength +
        StreamUtils.LengthOf(Text);

    public string Type { get; set; } = "markdown";
    public string Text { get; set; } = "";

    public override void ReadFrom(Stream stream)
    {
        base.ReadFrom(stream);
        Type = StreamUtils.ReadString(stream);
        Text = StreamUtils.ReadString(stream);
    }

    public override void WriteTo(Stream stream)
    {
        base.WriteTo(stream);
        StreamUtils.Write(stream, Type.AsSpan());
        StreamUtils.Write(stream, Text.AsSpan());
    }
}
