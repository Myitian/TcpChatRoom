using System.IO;
using System.Net.Sockets;

namespace TcpChatRoom.Network.Packet;

public abstract class Packet
{
    public abstract PacketType PacketType { get; }
    public virtual bool PreferWriteWithLength { get; } = false;

    public virtual int RawLength => StreamUtils.LengthOf(ID);
    public virtual Guid ID { get; set; } = Guid.NewGuid();

    public virtual void ReadFrom(Stream stream)
    {
        ID = StreamUtils.ReadGuid(stream);
    }
    public virtual void WriteTo(Stream stream)
    {
        StreamUtils.Write(stream, ID);
    }
    public virtual void WriteWithLength(Stream stream)
    {
        StreamUtils.Write(stream, RawLength);
        WriteTo(stream);
    }

    public static Packet? CreateFromRawPacket(RawPacket raw)
    {
        using MemoryWarpperStream ms = new(raw.Payload);
        Packet? packet = raw.PacketType switch
        {
            PacketType.Null => new NullPacket(),
            PacketType.Heartbeat => new HeartbeatPacket(),
            PacketType.Handshake => new HandshakePacket(),
            PacketType.Disconnect => new DisconnectPacket(),
            PacketType.Response => new ResponsePacket(),

            PacketType.Encrypted => new EncryptedPacket(),
            PacketType.Compressed => new CompressedPacket(),
            PacketType.MessageContainer => new MessageContainerPacket(),
            PacketType.MutipleContainer => new MutipleContainerPacket(),

            PacketType.TextMessage => new TextMessagePacket(),
            PacketType.BinaryMessage => new BinaryMessagePacket(),
            PacketType.RichTextMessage => new RichTextMessagePacket(),

            _ => null,
        };
        packet?.ReadFrom(ms);
        return packet;
    }
    public static Packet StaticReadFrom(Stream stream, Func<PacketType, int, ProcessPolicy>? processPolicy = null)
    {
        PacketType type = (PacketType)StreamUtils.ReadI32(stream);
        int length = StreamUtils.ReadI32(stream);
        Packet packet = processPolicy?.Invoke(type, length) switch
        {
            ProcessPolicy.Skip => new SkipPacket(type, length),
            ProcessPolicy.KeepRaw => new RawPacket(type, length),
            _ => type switch
            {
                PacketType.Null => new NullPacket(),
                PacketType.Heartbeat => new HeartbeatPacket(),
                PacketType.Handshake => new HandshakePacket(),
                PacketType.Disconnect => new DisconnectPacket(),
                PacketType.Response => new ResponsePacket(),

                PacketType.Encrypted => new EncryptedPacket(),
                PacketType.Compressed => new CompressedPacket(),
                PacketType.MessageContainer => new MessageContainerPacket(),
                PacketType.MutipleContainer => new MutipleContainerPacket(),

                PacketType.TextMessage => new TextMessagePacket(),
                PacketType.BinaryMessage => new BinaryMessagePacket(),
                PacketType.RichTextMessage => new RichTextMessagePacket(),

                _ => new RawPacket(type, length),
            }
        };
        packet.ReadFrom(stream);
        return packet;
    }
    public static void StaticWriteTo(Stream stream, Packet packet)
    {
        StreamUtils.Write(stream, (int)packet.PacketType);
        if (packet.PreferWriteWithLength)
            packet.WriteWithLength(stream);
        else
        {
            StreamUtils.Write(stream, packet.RawLength);
            packet.WriteTo(stream);
        }
    }
    public static Packet GetOriginalPacket(ISingleContainerPacket packet)
    {
        return GetOriginalPacket(packet.Body);
    }
    public static Packet GetOriginalPacket(Packet packet)
    {
        return packet switch
        {
            ISingleContainerPacket cp => GetOriginalPacket(cp.Body),
            _ => packet,
        };
    }
}
