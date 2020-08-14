using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using WebSocketSharp;

namespace SFGLib
{
    internal static class MessageHandler
    {
        internal static MessageType PacketIdToEnum(string packetId)
        {
            MessageType msgtype;
            switch (packetId)
            {
                case "SERVER_INIT": msgtype = MessageType.Init; break;
                case "SERVER_PLAYER_JOIN": msgtype = MessageType.PlayerJoin; break;
                case "SERVER_MOVEMENT": msgtype = MessageType.Movement; break;
                case "SERVER_BLOCK_SINGLE": msgtype = MessageType.BlockSingle; break;
                default: throw new NotImplementedException("unknown packet id " + packetId);
            }

            return msgtype;
        }

        internal static BaseMessage HandleMessage(MessageEventArgs m)
        {
            var raw = m.Data;
            var json = JsonConvert.DeserializeObject<JObject>(raw);
            string packetId = json["packetId"].ToString();
            MessageType msgtype;
            BaseMessage msg;
            msgtype = PacketIdToEnum(packetId);
            switch (msgtype)
            {
                case MessageType.Init:
                    {
                        var init = new InitMessage(raw);
                        JsonConvert.PopulateObject(raw, init);
                        init.Blocks = ReorderArrayRanks(init.Blocks);
                        msg = init;
                        break;
                    }
                case MessageType.PlayerJoin:
                //break;
                case MessageType.Movement:
                //break;
                case MessageType.BlockSingle:
                //break;
                default: throw new NotImplementedException();
            }
            return msg;
        }

        internal static T[,,] ReorderArrayRanks<T>(T[,,] array)
        {
            int l0 = array.GetLength(0), l1 = array.GetLength(1), l2 = array.GetLength(2);
            T[,,] newarr = new T[l0, l2, l1];

            for (int r0 = 0; r0 < l0; r0++)
                for (int r1 = 0; r1 < l1; r1++)
                    for (int r2 = 0; r2 < l2; r2++)
                        newarr[r0, r2, r1] = array[r0, r1, r2];

            return newarr;
        }
    }

    public abstract class BaseMessage
    {
        internal BaseMessage(MessageType type, string raw) { Type = type; Raw = raw; }
        public MessageType Type { get; }
        public string Raw { get; }
    }
    public sealed class InitMessage : BaseMessage
    {
        internal InitMessage(string raw) : base(MessageType.Init, raw) { }

        [JsonProperty("sender")]
        public int Sender { get; internal set; }
        [JsonProperty("spawnPosition")]
        public Point SpawnPosition { get; internal set; }
        [JsonProperty("size")]
        public Size WorldSize { get; internal set; }
        [JsonProperty("blocks")]
        public Block[,,] Blocks { get; internal set; }
    }
}
