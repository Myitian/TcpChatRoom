namespace TcpChatRoom.Network.Packet;

public class ResponsePacket : Packet
{
    public override PacketType PacketType => PacketType.Response;

    public bool IsSucceed { get; set; }
    public Guid ResponseID { get; set; }
    public string Message { get; set; } = "";

    public override int RawLength =>
        base.RawLength +
        StreamUtils.LengthOf(IsSucceed) +
        StreamUtils.LengthOf(ResponseID) +
        StreamUtils.LengthOf(Message.AsSpan());

    public override void ReadFrom(Stream stream)
    {
        base.ReadFrom(stream);
        IsSucceed = StreamUtils.ReadBool(stream);
        ResponseID = StreamUtils.ReadGuid(stream);
        Message = StreamUtils.ReadString(stream);
    }

    public override void WriteTo(Stream stream)
    {
        base.WriteTo(stream);
        StreamUtils.Write(stream, IsSucceed);
        StreamUtils.Write(stream, ResponseID);
        StreamUtils.Write(stream, Message.AsSpan());
    }
}
