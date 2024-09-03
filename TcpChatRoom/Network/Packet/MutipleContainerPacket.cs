using System.Security.Cryptography;

namespace TcpChatRoom.Network.Packet;

public class MutipleContainerPacket : Packet
{
    public override PacketType PacketType => PacketType.MutipleContainer;
    public override bool PreferWriteWithLength => Body.Any(p => p.PreferWriteWithLength);

    public override int RawLength =>
        base.RawLength +
        StreamUtils.LengthOf(Body.Length) +
        Body.Select(x => x.RawLength + 24).Sum();
    public Packet[] Body { get; set; } = [];

    public override void ReadFrom(Stream stream)
    {
        base.ReadFrom(stream);
        int length = StreamUtils.ReadI32(stream);
        Body = new Packet[length];
        for (int i = 0; i < length; i++)
        {
            Body[i] = StaticReadFrom(stream);
        }
    }

    public override void WriteTo(Stream stream)
    {
        base.WriteTo(stream);
        StreamUtils.Write(stream, Body.Length);
        for (int i = 0; i < Body.Length; i++)
        {
            StaticWriteTo(stream, Body[i] ?? new NullPacket());
        }
    }

    public override void WriteWithLength(Stream stream)
    {
        using MemoryStream ms = new();
        for (int i = 0; i < Body.Length; i++)
        {
            StaticWriteTo(ms, Body[i] ?? new NullPacket());
        }
        int length = StreamUtils.LengthOf(Body.Length) + (int)ms.Length + base.RawLength;
        StreamUtils.Write(stream, length);
        base.WriteTo(stream);
        StreamUtils.Write(stream, Body.Length);
        ms.Position = 0;
        ms.CopyTo(stream);
    }
}
