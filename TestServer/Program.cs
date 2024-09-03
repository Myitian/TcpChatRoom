using System.Net;
using System.Text;
using TcpChatRoom.Server;

Console.ResetColor();
Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("Hello, World!");
using ServerMain server = new(IPEndPoint.Parse("0.0.0.0:23333"), IPEndPoint.Parse("[::]:23333"));
Console.CancelKeyPress += Cancel;
server.Wait();

void Cancel(object? sender, ConsoleCancelEventArgs e)
{
    Console.WriteLine("EXITING...");
    e.Cancel = true;
    server.Dispose();
    Console.ResetColor();
    Console.CancelKeyPress -= Cancel;
};