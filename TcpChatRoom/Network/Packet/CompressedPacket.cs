using System.IO.Compression;

namespace TcpChatRoom.Network.Packet;

public class CompressedPacket : Packet, ISingleContainerPacket
{
    public override PacketType PacketType => PacketType.Compressed;
    public override bool PreferWriteWithLength => true;

    public override int RawLength
    {
        get
        {
            using MemoryStream ms = new();
            using (Stream cs = CreateStream(ms, AlgorithmName, CompressionLevel, CompressionMode.Compress))
                StaticWriteTo(cs, Body);
            return
                base.RawLength +
                StreamUtils.LengthOf(AlgorithmName.AsSpan()) +
                (int)ms.Length;
        }
    }

    public string AlgorithmName { get; set; } = "deflate";
    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Optimal;
    public Packet Body { get; set; } = new NullPacket();

    public override void ReadFrom(Stream stream)
    {
        base.ReadFrom(stream);
        AlgorithmName = StreamUtils.ReadString(stream);
        using Stream cs = CreateStream(stream, AlgorithmName, CompressionLevel, CompressionMode.Decompress);
        Body = StaticReadFrom(cs);
    }

    public override void WriteTo(Stream stream)
    {
        base.WriteTo(stream);
        StreamUtils.Write(stream, AlgorithmName.AsSpan());
        using Stream cs = CreateStream(stream, AlgorithmName, CompressionLevel, CompressionMode.Compress);
        StaticWriteTo(cs, Body);
    }

    public override void WriteWithLength(Stream stream)
    {
        using MemoryStream ms = new();
        using (Stream cs = CreateStream(ms, AlgorithmName, CompressionLevel, CompressionMode.Compress))
            StaticWriteTo(cs, Body);
        int length = StreamUtils.LengthOf(AlgorithmName.AsSpan()) + (int)ms.Length + base.RawLength;
        StreamUtils.Write(stream, length);
        base.WriteTo(stream);
        StreamUtils.Write(stream, AlgorithmName.AsSpan());
        ms.Position = 0;
        ms.CopyTo(stream);
    }

    public static Stream CreateStream(Stream stream, string algName, CompressionLevel level, CompressionMode mode)
    {
        return algName.ToLower() switch
        {
            "deflate" => mode is CompressionMode.Compress ?
                                new DeflateStream(stream, level, true) :
                                new DeflateStream(stream, CompressionMode.Decompress, true),
            "gz" or "gzip" => mode is CompressionMode.Compress ?
                                new GZipStream(stream, level, true) :
                                new GZipStream(stream, CompressionMode.Decompress, true),
            "zlib" => mode is CompressionMode.Compress ?
                                new ZLibStream(stream, level, true) :
                                new ZLibStream(stream, CompressionMode.Decompress, true),
            "br" or "brotli" => mode is CompressionMode.Compress ?
                                new BrotliStream(stream, level, true) :
                                new BrotliStream(stream, CompressionMode.Decompress, true),
            _ => throw new NotSupportedException(),
        };
    }
}
