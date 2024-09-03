namespace TcpChatRoom.Network.Packet;

public enum PacketType
{
    // Control
    Null = 0x0000,
    Heartbeat = 0x0001,
    Handshake = 0x0002,
    Disconnect = 0x0003,
    Response = 0x0004,
    // Single Container
    Encrypted = 0x1000,
    Compressed = 0x1001,
    MessageContainer = 0x1002,
    // Mutiple Container
    MutipleContainer = 0x2000,
    // Message
    TextMessage = 0x3000,
    BinaryMessage = 0x3001,
    RichTextMessage = 0x3002,
}
