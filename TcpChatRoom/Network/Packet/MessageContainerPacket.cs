using System.IO.Compression;

namespace TcpChatRoom.Network.Packet;

public class MessageContainerPacket : MessagePacket, ISingleContainerPacket
{
    public override PacketType PacketType => PacketType.Compressed;
    public override bool PreferWriteWithLength => Body.PreferWriteWithLength;

    public override int RawLength =>
        base.RawLength +
        Body.RawLength + 24;

    public Packet Body { get; set; } = new NullPacket();

    public override void ReadFrom(Stream stream)
    {
        base.ReadFrom(stream);
        Body = StaticReadFrom(stream);
    }

    public override void WriteTo(Stream stream)
    {
        base.WriteTo(stream);
        StaticWriteTo(stream, Body);
    }

    public override void WriteWithLength(Stream stream)
    {
        using MemoryStream ms = new();
        StaticWriteTo(ms, Body);
        StreamUtils.Write(stream, (int)ms.Length + base.RawLength);
        base.WriteTo(stream);
        ms.Position = 0;
        ms.CopyTo(stream);
    }
}
