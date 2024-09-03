namespace TcpChatRoom.Network.Packet;

public abstract class MessagePacket : Packet
{
    public virtual DateTimeOffset MessageTime { get; set; } = DateTimeOffset.Now;
    public virtual string? MessageAuthor { get; set; }
    public override int RawLength =>
        base.RawLength +
        StreamUtils.LengthOf(ID) +
        StreamUtils.LengthOf(MessageTime) +
        StreamUtils.LengthOf(MessageAuthor);

    public override void ReadFrom(Stream stream)
    {
        base.ReadFrom(stream);
        ID = StreamUtils.ReadGuid(stream);
        MessageTime = StreamUtils.ReadDateTimeOffset(stream);
        MessageAuthor = StreamUtils.ReadNullableString(stream);
    }
    public override void WriteTo(Stream stream)
    {
        base.WriteTo(stream);
        StreamUtils.Write(stream, ID);
        StreamUtils.Write(stream, MessageTime);
        StreamUtils.Write(stream, MessageAuthor);
    }
}
