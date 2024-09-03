using System.Buffers;

namespace TcpChatRoom.Network.Packet;

public class BinaryMessagePacket : MessagePacket
{
    public override PacketType PacketType => PacketType.BinaryMessage;

    public override int RawLength =>
        base.RawLength +
        StreamUtils.LengthOf(ContentType) +
        StreamUtils.LengthOf(Name) +
        StreamUtils.LengthOf(Offset) +
        StreamUtils.LengthOf(TotalLength) +
        StreamUtils.LengthOf(Payload ?? MemoryBuffer<byte>.Empty);

    public Guid FileID { get; set; }
    public string ContentType { get; set; } = "";
    public string Name { get; set; } = "";
    public long Offset { get; set; }
    public long TotalLength { get; set; }
    public MemoryBuffer<byte>? Payload { get; set; }

    public override void ReadFrom(Stream stream)
    {
        base.ReadFrom(stream);
        FileID = StreamUtils.ReadGuid(stream);
        ContentType = StreamUtils.ReadString(stream);
        Name = StreamUtils.ReadString(stream);
        Offset = StreamUtils.ReadI64(stream);
        TotalLength = StreamUtils.ReadI64(stream);
        Payload = StreamUtils.ReadByteArrayAsMemoryBuffer(stream);
    }

    public override void WriteTo(Stream stream)
    {
        base.WriteTo(stream);
        StreamUtils.Write(stream, FileID);
        StreamUtils.Write(stream, ContentType.AsSpan());
        StreamUtils.Write(stream, Name.AsSpan());
        StreamUtils.Write(stream, Offset);
        StreamUtils.Write(stream, TotalLength);
        StreamUtils.Write(stream, Payload ?? MemoryBuffer<byte>.Empty);
    }
}
