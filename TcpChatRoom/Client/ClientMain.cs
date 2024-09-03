using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using TcpChatRoom.Network;
using TcpChatRoom.Network.Packet;
using Timer = System.Timers.Timer;

namespace TcpChatRoom.Client;

public class ClientMain : IDisposable
{
    private bool disposed = false;
    private bool stopped = false;
    private readonly Thread readThread;
    private readonly Thread writeThread;
    private readonly ConnectionInfo conn;
    private readonly ConcurrentDictionary<Guid, Action<Guid, ResponsePacket?>?> responseCallbacks = [];

#if DEBUG
    public const int TIMEOUT = 10000000;
#else
    public const int TIMEOUT = 10000;
#endif
    public const int HALF_TIMEOUT = TIMEOUT / 2;

    public static readonly long Version = 0x000_000_000_000;

    public Action<MessagePacket>? OnMessageReceived { get; set; }

    private void OnMessageReceivedInternal(MessagePacket message)
    {
        OnMessageReceived?.Invoke(message);
    }
    private void OnResponseReceivedInternal(ResponsePacket response)
    {
        if (responseCallbacks.Remove(response.ResponseID, out Action<Guid, ResponsePacket?>? callback))
            callback?.Invoke(response.ResponseID, response);
    }

    private void ReadThread()
    {
        Thread.CurrentThread.Name = "ReadThread";
        ClientSidePacketHandler handler = new()
        {
            Conn = conn,
            Version = Version,
            OnMessageReceived = OnMessageReceivedInternal,
            OnResponseReceived = OnResponseReceivedInternal
        };
        try
        {
            while (!stopped)
            {
                Packet ipacket = Packet.StaticReadFrom(conn.NetworkStream, ClientSidePacketHandler.DefaultPacketProcessPolicy);
                if (handler.ProcessPacket(ipacket))
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message);
        }
        conn.SendingPacketQueue.CompleteAdding();
        conn.Client.Close();
        return;
    }
    private void WriteThread()
    {
        Thread.CurrentThread.Name = "WriteThread";
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

    public void Handshake(string? name, string? helloMessage = null, string? auth = null)
    {
        conn.SendingPacketQueue.Add(new HandshakePacket()
        {
            Name = name,
            HelloMessage = helloMessage,
            Authentication = auth
        });
    }
    public void Send(
        MessagePacket packet,
        Action<Guid, ResponsePacket?>? responseCallback = null,
        TimeSpan? responseTimeout = null)
    {
        Guid pid = packet.ID;
        if (responseCallback is not null)
        {
            responseCallbacks[pid] = responseCallback;
            if (responseTimeout >= TimeSpan.Zero)
            {
                Timer timer = new(responseTimeout.Value);
                timer.Elapsed += (sender, _) =>
                {
                    if (responseCallbacks.Remove(pid, out Action<Guid, ResponsePacket?>? callback))
                        callback?.Invoke(pid, null);
                    if (sender is IDisposable d)
                        d.Dispose();
                };
                timer.AutoReset = false;
                timer.Enabled = true;
            }
        }
        conn.SendingPacketQueue.Add(packet);
    }

    public ClientMain(IPEndPoint server)
    {
        TcpClient client = new()
        {
            LingerState = new(true, 5000),
            NoDelay = true
        };
        client.Connect(server);
        conn = new(client);
        readThread = new(ReadThread);
        writeThread = new(WriteThread);
        readThread.Start();
        writeThread.Start();
    }

    public ClientMain(string hostname, ushort port)
    {
        TcpClient client = new()
        {
            LingerState = new(true, 5000),
            NoDelay = true
        };
        client.Connect(hostname, port);
        conn = new(client);
        readThread = new(ReadThread);
        writeThread = new(WriteThread);
        readThread.Start();
        writeThread.Start();
    }

    public void Wait()
    {
        readThread.Join();
        writeThread.Join();
    }
    public void Close()
    {
        conn.SendingPacketQueue.Add(new DisconnectPacket()
        {
            Reason = "客户端关闭"
        });
        conn.SendingPacketQueue.CompleteAdding();
        writeThread.Join();
        stopped = true;
        readThread.Join();
    }
    public void Kill()
    {
        conn.Client.Close();
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
