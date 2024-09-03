using System.IO.Compression;
using System.Security.Cryptography;

namespace TcpChatRoom.Network.Packet;

public class EncryptedPacket : Packet, ISingleContainerPacket
{

    private static readonly Aes aes = Aes.Create();
    static EncryptedPacket()
    {
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
    }

    public override PacketType PacketType => PacketType.Encrypted;
    public override bool PreferWriteWithLength => Body.PreferWriteWithLength;

    public override int RawLength =>
        base.RawLength +
        StreamUtils.LengthOf(Key) +
        StreamUtils.LengthOf(IV) +
        aes.GetCiphertextLengthCbc(Body.RawLength + 24);

    public byte[]? Key { get; set; }
    public byte[]? IV { get; set; }
    public Packet Body { get; set; } = new NullPacket();

    public override void ReadFrom(Stream stream)
    {
        base.ReadFrom(stream);
        Key = StreamUtils.ReadByteArray(stream);
        IV = StreamUtils.ReadByteArray(stream);
        using CryptoStream cs = CreateCryptoStream(stream, Key, IV, CryptoStreamMode.Read);
        Body = StaticReadFrom(cs);
    }

    public override void WriteTo(Stream stream)
    {
        base.WriteTo(stream);
        if (Key is null)
        {
            Key = new byte[32];
            RandomNumberGenerator.Fill(Key);
        }
        if (IV is null)
        {
            IV = new byte[16];
            RandomNumberGenerator.Fill(IV);
        }
        StreamUtils.Write(stream, Key);
        StreamUtils.Write(stream, IV);
        using CryptoStream cs = CreateCryptoStream(stream, Key, IV, CryptoStreamMode.Write);
        StaticWriteTo(cs, Body);
    }

    public override void WriteWithLength(Stream stream)
    {
        if (Key is null)
        {
            Key = new byte[32];
            RandomNumberGenerator.Fill(Key);
        }
        if (IV is null)
        {
            IV = new byte[16];
            RandomNumberGenerator.Fill(IV);
        }
        using MemoryStream ms = new();
        using (CryptoStream cs = CreateCryptoStream(ms, Key, IV, CryptoStreamMode.Write))
            StaticWriteTo(cs, Body);
        int length = StreamUtils.LengthOf(Key) + StreamUtils.LengthOf(IV) + (int)ms.Length + base.RawLength;
        StreamUtils.Write(stream, length);
        base.WriteTo(stream);
        StreamUtils.Write(stream, Key);
        StreamUtils.Write(stream, IV);
        ms.Position = 0;
        ms.CopyTo(stream);
    }

    public static CryptoStream CreateCryptoStream(Stream stream, byte[] key, byte[] iv, CryptoStreamMode mode)
    {
        return new(
            stream,
            mode is CryptoStreamMode.Read ?
                aes.CreateDecryptor(key, iv) :
                aes.CreateEncryptor(key, iv),
            mode,
            true);
    }
}
