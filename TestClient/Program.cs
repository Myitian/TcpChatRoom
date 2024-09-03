using System.Text;
using TcpChatRoom;
using TcpChatRoom.Client;
using TcpChatRoom.Network.Packet;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
Console.ForegroundColor = ConsoleColor.Gray;
Console.WriteLine("TcpChatRoom 客户端，按 Ctrl+C 退出");
Console.WriteLine("服务器地址");
string? hostStr = Console.ReadLine();
Console.WriteLine("服务器端口");
string? portStr = Console.ReadLine();
Console.WriteLine("用户名");
string? name = Console.ReadLine();

bool closed = false;
string host = string.IsNullOrEmpty(hostStr) ? "localhost" : hostStr;
ushort port;
if (string.IsNullOrEmpty(portStr) || !ushort.TryParse(portStr, out port))
    port = 23333;
Console.WriteLine($"""

    以
    {name}
    的身份连接到服务器
    {host}
    {port}

    连接中……
    """);
using ClientMain client = new(host, port);
client.Handshake(name);
Console.CancelKeyPress += Cancel;
while (!closed)
{
    string? text = Console.ReadLine();
    if (text is null)
        continue;
    lock (Logger.LoggerLock)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        TextMessagePacket message = new()
        {
            MessageTime = DateTimeOffset.Now,
            Text = text
        };
        Console.WriteLine($"发送消息：{message.ID}");
        client.Send(message, Response, TimeSpan.FromMilliseconds(ClientMain.HALF_TIMEOUT));
    }
}

void Cancel(object? sender, ConsoleCancelEventArgs e)
{
    closed = true;
    lock (Logger.LoggerLock)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("正在退出……");
    }
    client.Dispose();
    Console.ResetColor();
    e.Cancel = true;
    Console.CancelKeyPress -= Cancel;
    Environment.Exit(0);
};
void Response(Guid id, ResponsePacket? resp)
{
    if (resp is null)
        Console.WriteLine($"消息发送失败：{id}，响应超时");
    else if (!resp.IsSucceed)
        Console.WriteLine($"消息发送失败：{id}，原因：{resp.Message}");
}