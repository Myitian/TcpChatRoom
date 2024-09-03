using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TcpChatRoom.Network;
using TcpChatRoom.Network.Packet;

namespace TcpChatRoom.Server
{
    public class ServerSidePacketHandler
    {
        public required ConnectionInfo Conn { get; set; }
        public Action<Packet, TcpClient?>? BoardcastPacket { get; set; }
        public long Version { get; set; }
        public string? Name { get; set; }
        public string? HelloMessage { get; set; }

        public static ProcessPolicy DefaultPacketProcessPolicy(PacketType type, int length)
        {
            if (length > 4 * 1024 * 1024)
                return ProcessPolicy.Skip;
            if (((int)type >> 12) == 0x1)
                return ProcessPolicy.KeepRaw;
            return ProcessPolicy.Parse;
        }

        public void LogMessage(Packet? packet, string? msg)
        {
            if (packet is null)
                Logger.Information($"【{Name}】：" + msg);
            else
                Logger.Information($"【{Name}】{packet.ID};{packet.PacketType}：" + msg);
        }

        public bool ProcessPacket(Packet ipacket, RawPacket? raw = null)
        {
            return ipacket switch
            {
                NullPacket
                    => false,
                HeartbeatPacket packet
                    => ProcessHeartbeatPacket(packet),
                HandshakePacket packet
                    => ProcessHandshakePacket(packet),
                DisconnectPacket packet
                    => ProcessDisconnectPacket(packet),
                ResponsePacket
                    => false,

                MessageContainerPacket packet
                    => ProcessMessageContainerPacket(packet, raw),
                ISingleContainerPacket packet
                    => ProcessISingleContainerPacket(packet, raw),
                MutipleContainerPacket packet
                    => ProcessMutipleContainerPacketPacket(packet),

                MessagePacket packet
                    => ProcessMessagePacket(packet, raw),

                RawPacket packet
                    => ProcessRawPacket(packet, raw),
                SkipPacket packet
                    => ProcessSkipPacket(packet),
                _
                    => false,
            };
        }

        public bool ProcessHeartbeatPacket(HeartbeatPacket packet)
        {
            if (packet.ShouldResponse)
            {
                Conn.SendingPacketQueue.Add(new HeartbeatPacket()
                {
                    ShouldResponse = false
                });
            }
            return false;
        }
        public bool ProcessHandshakePacket(HandshakePacket packet)
        {
            if (packet.Name is not null)
                Name = $"{packet.Name}@{Conn.StringIP}";
            string msg = packet.HelloMessage ?? "加入了服务器";
            string hello = $"【{Name}】{msg}！";
            LogMessage(packet, $"握手;{msg}");
            Conn.SendingPacketQueue.Add(new HandshakePacket()
            {
                HelloMessage = HelloMessage,
                Version = Version
            });
            BoardcastPacket?.Invoke(new TextMessagePacket()
            {
                MessageAuthor = null,
                Text = hello
            }, Conn.Client);
            return false;
        }
        public bool ProcessDisconnectPacket(DisconnectPacket packet)
        {
            if (packet.Reason is null)
                LogMessage(packet, $"断开连接");
            else
                LogMessage(packet, $"断开连接，原因：{packet.Reason}");
            BoardcastPacket?.Invoke(new TextMessagePacket()
            {
                MessageAuthor = null,
                Text = $"【{Name}】退出了服务器！"
            }, Conn.Client);
            return true;
        }
        public bool ProcessMessageContainerPacket(MessageContainerPacket packet, RawPacket? raw = null)
        {
            Packet original = Packet.GetOriginalPacket(packet as ISingleContainerPacket);
            if (original is not MessagePacket mp)
                return false;
            mp.ID = packet.ID;
            mp.MessageTime = packet.MessageTime;
            mp.MessageAuthor = packet.MessageAuthor;
            return ProcessMessagePacket(mp, raw);
        }
        public bool ProcessISingleContainerPacket(ISingleContainerPacket packet, RawPacket? raw = null)
        {
            Packet original = Packet.GetOriginalPacket(packet);
            return ProcessPacket(original, raw);
        }
        public bool ProcessMutipleContainerPacketPacket(MutipleContainerPacket packet)
        {
            foreach (Packet ipacket in packet.Body)
            {
                if (ProcessPacket(ipacket))
                    return true;
            }
            return false;
        }
        public bool ProcessMessagePacket(MessagePacket packet, RawPacket? raw = null)
        {
            string? msg = packet switch
            {
                TextMessagePacket text => $"文本消息;Length={text.Text.Length};{text.Text}",
                BinaryMessagePacket bin => $"二进制消息;Type={bin.ContentType};Length={bin.Payload?.Length};Offset={bin.Offset};Total={bin.TotalLength}",
                RichTextMessagePacket rt => $"富文本消息;Type={rt.Type};Length={rt.Text.Length}",
                _ => "未知消息"
            };
            LogMessage(packet, msg);
            Conn.SendingPacketQueue.Add(new ResponsePacket()
            {
                IsSucceed = true,
                ResponseID = packet.ID
            });
            if (raw is null)
            {
                packet.MessageAuthor = Name;
                BoardcastPacket?.Invoke(packet, Conn.Client);
            }
            else
            {
                BoardcastPacket?.Invoke(new MessageContainerPacket()
                {
                    ID = packet.ID,
                    MessageTime = packet.MessageTime,
                    MessageAuthor = Name,
                    Body = raw,
                }, Conn.Client);
            }
            return false;
        }
        public bool ProcessRawPacket(RawPacket packet, RawPacket? raw = null)
        {
            Packet? original = Packet.CreateFromRawPacket(packet);
            return original is not null && ProcessPacket(original, raw);
        }
        public bool ProcessSkipPacket(SkipPacket packet)
        {
            Conn.SendingPacketQueue.Add(new ResponsePacket()
            {
                IsSucceed = false,
                ResponseID = packet.ID,
                Message = "数据包过大！"
            });
            LogMessage(packet, "（已丢弃）");
            return false;
        }
    }
}
