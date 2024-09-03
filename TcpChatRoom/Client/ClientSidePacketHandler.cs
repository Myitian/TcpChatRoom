using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TcpChatRoom.Network;
using TcpChatRoom.Network.Packet;

namespace TcpChatRoom.Client
{
    public class ClientSidePacketHandler
    {
        public required ConnectionInfo Conn { get; set; }
        public long Version { get; set; }
        public string? Name { get; set; }
        public string? HelloMessage { get; set; }
        public Action<MessagePacket>? OnMessageReceived { get; set; }
        public Action<ResponsePacket>? OnResponseReceived { get; set; }

        public static ProcessPolicy DefaultPacketProcessPolicy(PacketType type, int length)
        {
            if (length > 4 * 1024 * 1024)
                return ProcessPolicy.Skip;
            return ProcessPolicy.Parse;
        }

        public void LogMessage(Packet? packet, string? msg)
        {
            if (packet is MessagePacket mp)
                Logger.Information($"【{mp.MessageAuthor}】{packet.ID};{packet.PacketType}：" + msg);
            else if (packet is null)
                Logger.Information($"【System】：" + msg);
            else
                Logger.Information($"【System】{packet.ID};{packet.PacketType}：" + msg);
        }

        public bool ProcessPacket(Packet ipacket)
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
                ResponsePacket packet
                    => ProcessResponsePacket(packet),

                MessageContainerPacket packet
                    => ProcessMessageContainerPacket(packet),
                ISingleContainerPacket packet
                    => ProcessISingleContainerPacket(packet),
                MutipleContainerPacket packet
                    => ProcessMutipleContainerPacketPacket(packet),

                MessagePacket packet
                    => ProcessMessagePacket(packet),

                RawPacket packet
                    => ProcessRawPacket(packet),
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
            string msg = packet.HelloMessage ?? "欢迎加入服务器";
            LogMessage(packet, $"握手;{msg}");
            OnMessageReceived?.Invoke(new TextMessagePacket()
            {
                MessageAuthor = packet.Name,
                Text = msg
            });
            return false;
        }
        public bool ProcessDisconnectPacket(DisconnectPacket packet)
        {
            if (packet.Reason is null)
                LogMessage(packet, $"断开连接");
            else
                LogMessage(packet, $"断开连接，原因：{packet.Reason}");
            return true;
        }
        public bool ProcessResponsePacket(ResponsePacket packet)
        {
            OnResponseReceived?.Invoke(packet);
            return false;
        }
        public bool ProcessMessageContainerPacket(MessageContainerPacket packet)
        {
            Packet original = Packet.GetOriginalPacket(packet as ISingleContainerPacket);
            if (original is not MessagePacket mp)
                return false;
            mp.ID = packet.ID;
            mp.MessageTime = packet.MessageTime;
            mp.MessageAuthor = packet.MessageAuthor;
            return ProcessMessagePacket(mp);
        }
        public bool ProcessISingleContainerPacket(ISingleContainerPacket packet)
        {
            Packet original = Packet.GetOriginalPacket(packet);
            return ProcessPacket(original);
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
        public bool ProcessMessagePacket(MessagePacket packet)
        {
            string? msg = packet switch
            {
                TextMessagePacket text => $"文本消息;Length={text.Text.Length};{text.Text}",
                BinaryMessagePacket bin => $"二进制消息;Type={bin.ContentType};Length={bin.Payload?.Length ?? 0};Offset={bin.Offset};Total={bin.TotalLength}",
                RichTextMessagePacket rt => $"富文本消息;Type={rt.Type};Length={rt.Text.Length}",
                _ => "未知消息"
            };
            LogMessage(packet, msg);
            Conn.SendingPacketQueue.Add(new ResponsePacket()
            {
                IsSucceed = true,
                ResponseID = packet.ID
            });
            OnMessageReceived?.Invoke(packet);
            return false;
        }
        public bool ProcessRawPacket(RawPacket packet)
        {
            Packet? original = Packet.CreateFromRawPacket(packet);
            return original is not null && ProcessPacket(original);
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
