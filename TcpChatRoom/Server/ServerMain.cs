using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Xml.Linq;
using TcpChatRoom.Network;
using TcpChatRoom.Network.Packet;

namespace TcpChatRoom.Server;

public class ServerMain : IDisposable
{
    private bool disposed = false;
    private bool stopped = false;
    private int acceptedConnectionCount= 0;
    private readonly Thread[] threads;
    private readonly TcpListener[] listeners;
    private readonly ConcurrentDictionary<TcpClient, ConnectionInfo> clients = [];

#if DEBUG
    public const int TIMEOUT = 10000000;
#else
    public const int TIMEOUT = 5000;
#endif
    public const int HALF_TIMEOUT = TIMEOUT / 2;

    public static readonly long Version = 0x000_000_000_000;

    private void Listen(object? arg)
    {
        if (arg is not TcpListener listener)
        {
            Logger.Error("arg is not an TcpListener");
            return;
        }
        listener.Start();
        try
        {
            while (!stopped)
            {
                TcpClient client = listener.AcceptTcpClient();
                try
                {
                    client.LingerState = new(true, 5000);
                    client.NoDelay = true;
                    client.ReceiveTimeout = TIMEOUT;
                    client.SendTimeout = TIMEOUT;
                    ConnectionInfo conn = new(client, acceptedConnectionCount++);
                    clients[client] = conn;
                    bool v1 = ThreadPool.QueueUserWorkItem(ReadThread, conn);
                    bool v2 = ThreadPool.QueueUserWorkItem(WriteThread, conn);
                    if (!(v1 && v2))
                        goto Err;
                }
                catch
                {
                    goto Err;
                }
                continue;
            Err:
                Logger.Error("Cannot invoke QueueUserWorkItem");
            }
        }
        catch (SocketException)
        {
            Logger.Information("Exiting...");
        }
        finally
        {
            listener.Stop();
        }
    }
    private void ReadThread(object? arg)
    {
        if (arg is not ConnectionInfo conn)
        {
            Logger.Error("arg is not a TcpClient");
            return;
        }
        Thread.CurrentThread.Name = $"ReadThread-{conn.ID}";
        ServerSidePacketHandler handler = new()
        {
            Conn = conn,
            Name = $"未知用户@{conn.StringIP}",
            HelloMessage = "欢迎加入服务器！",
            BoardcastPacket = BoardcastPacket,
            Version = Version
        };
        bool passiveExit = true;
        try
        {
            while (!stopped)
            {
                Packet ipacket = Packet.StaticReadFrom(
                    conn.NetworkStream,
                    ServerSidePacketHandler.DefaultPacketProcessPolicy);
                if (handler.ProcessPacket(ipacket))
                {
                    passiveExit = false;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
        }
        if (passiveExit)
        {
            handler.LogMessage(null, "断开连接（掉线）");
            BoardcastPacket(new TextMessagePacket()
            {
                MessageAuthor = null,
                Text = $"【{handler.Name}】退出了服务器！"
            }, conn.Client);
        }
        conn.Client.Close();
        clients.Remove(conn.Client, out _);
        conn.SendingPacketQueue.CompleteAdding();
        return;
    }
    private void WriteThread(object? arg)
    {
        if (arg is not ConnectionInfo conn)
        {
            Logger.Error("arg is not a TcpClient");
            return;
        }
        Thread.CurrentThread.Name = $"WriteThread-{conn.ID}";
        try
        {
            while (!stopped)
            {
                Packet? ipacket = null;
                try
                {
                    if (!conn.SendingPacketQueue.TryTake(out ipacket, HALF_TIMEOUT))
                    {
                        conn.SendingPacketQueue.Add(new HeartbeatPacket() { ShouldResponse = true });
                    }
                }
                catch (InvalidOperationException)
                {
                    Logger.Information("Completed!");
                    break;
                }
                if (ipacket is null)
                    continue;
                Packet.StaticWriteTo(conn.NetworkStream, ipacket);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message);
        }
    }
    private void BoardcastPacket(Packet packet, TcpClient? excluded = null)
    {
        foreach ((TcpClient client, ConnectionInfo info) in clients)
        {
            if (client == excluded)
                continue;
            if (!info.SendingPacketQueue.TryAdd(packet))
            {
                Logger.Warning($"Failed to add packet to {info.StringIP}");
            }
        }
    }

    public ServerMain(params ReadOnlySpan<IPEndPoint> listeningEndPoints)
    {
        threads = new Thread[listeningEndPoints.Length];
        listeners = new TcpListener[listeningEndPoints.Length];
        for (int i = 0; i < listeningEndPoints.Length; i++)
        {
            TcpListener listener = new(listeningEndPoints[i]);
            listeners[i] = listener;
            Thread thread = new(Listen)
            {
                Name = $"ServerListen-{i}"
            };
            thread.Start(listener);
            threads[i] = thread;
        }
    }

    public void Wait()
    {
        foreach (var thread in threads)
        {
            thread.Join();
        }
    }
    public void Close()
    {
        BoardcastPacket(new DisconnectPacket()
        {
            Reason = "服务器关闭！"
        });
        stopped = true;
        Kill();
        foreach (var thread in threads)
        {
            thread.Join();
        }
    }
    public void Kill()
    {
        foreach (var listener in listeners)
        {
            listener.Stop();
        }
    }

    private void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                Close();
            }
            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
