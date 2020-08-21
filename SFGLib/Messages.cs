using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using WebSocketSharp;

namespace SFGLib
{
    internal static class MessageParser
    {
        internal static MessageType PacketIdToEnum(string packetId)
        {
            switch (packetId)
            {
                case "SERVER_INIT": return MessageType.Init;
                case "SERVER_PLAYER_JOIN": return MessageType.PlayerJoin;
                case "SERVER_PLAYER_LEAVE": return MessageType.PlayerLeave;
                case "SERVER_MOVEMENT": return MessageType.Movement;
                case "SERVER_BLOCK_SINGLE": return MessageType.BlockSingle;
                case "SERVER_BLOCK_LINE": return MessageType.BlockLine;
                case "SERVER_BLOCK_BUFFER": return MessageType.BlockBuffer;
                case "SERVER_FIRE_BULLET": return MessageType.FireBullet;
                case "SERVER_PICKUP_GUN": return MessageType.PickupGun;
                case "SERVER_EQUIP_GUN": return MessageType.EquipGun;
                default: throw new NotImplementedException("unknown packet id " + packetId);
            }
        }
        internal static string EnumToPacketId(MessageType type)
        {
            switch (type)
            {
                case MessageType.Movement: return "MOVEMENT";
                case MessageType.BlockSingle: return "BLOCK_SINGLE";
                case MessageType.BlockLine: return "BLOCK_LINE";
                case MessageType.BlockBuffer: return "BLOCK_BUFFER";
                case MessageType.PickupGun: return "PICKUP_GUN";
                case MessageType.EquipGun: return "EQUIP_GUN";
                case MessageType.FireBullet: return "FIRE_BULLET";
                default: throw new NotImplementedException("not convertible from enum " + type);
            }
        }

        internal static T CreateMessage<T>(string raw) where T : BaseMessage, new()
        {
            var m = new T() { Raw = raw };
            JsonConvert.PopulateObject(raw, m);
            return m;
        }

        internal static BaseMessage ParseMessage(MessageEventArgs m) => ParseMessage(m.Data);
        internal static BaseMessage ParseMessage(string raw)
        {
            var json = JsonConvert.DeserializeObject<JObject>(raw);
            string packetId = json["packetId"].ToString();

            BaseMessage msg;
            var msgtype = PacketIdToEnum(packetId);
            switch (msgtype)
            {
                case MessageType.Init:
                    {
                        var init = CreateMessage<InitMessage>(raw);
                        init.Blocks = ReorderArrayRanks(init.Blocks);
                        msg = init;
                        break;
                    }
                case MessageType.PlayerJoin: msg = CreateMessage<PlayerJoinMessage>(raw); break;
                case MessageType.PlayerLeave: msg = CreateMessage<PlayerLeaveMessage>(raw); break;
                case MessageType.Movement: msg = CreateMessage<MovementMessage>(raw); break;
                case MessageType.BlockSingle: msg = CreateMessage<BlockSingleMessage>(raw); break;
                case MessageType.BlockLine: msg = CreateMessage<BlockLineMessage>(raw); break;
                case MessageType.BlockBuffer:
                    {
                        var buf = CreateMessage<BlockBufferMessage>(raw);
                        var obj = JsonConvert.DeserializeObject<JObject>(raw);
                        var arr = obj["blocks"] as JArray;
                        var count = arr.Count;
                        BaseMessage[] msgs = new BaseMessage[count];
                        for (int i = 0; i < count; i++)
                        {
                            var v = ParseMessage(arr[i].ToString());
                            if (v.Type != MessageType.BlockSingle && v.Type != MessageType.BlockLine) throw new ArgumentException("blockbuffer msg contains invalid child");
                            msgs[i] = v;
                        }
                        buf.Blocks = msgs;
                        msg = buf;
                        break;
                    }
                case MessageType.FireBullet: msg = CreateMessage<FireBulletMessage>(raw); break;
                case MessageType.PickupGun: msg = CreateMessage<PickupGunMessage>(raw); break;
                case MessageType.EquipGun: msg = CreateMessage<EquipGunMessage>(raw); break;
                default: msg = new BaseMessage() { Raw = raw }; break;
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

    public interface IPlayerId {[JsonProperty("playerId")] int PlayerId { get; } }
    public interface IPositioned {[JsonProperty("position")] Point Position { get; } }

    public class BaseMessage
    {
        internal BaseMessage() { }

        public virtual MessageType Type { get; }
        internal string packetId => Type != MessageType.Unknown ? MessageParser.EnumToPacketId(Type) : JsonConvert.DeserializeAnonymousType(Raw, new { packetId = default(string) }).packetId;
        public string Raw { get; internal set; }
    }

    public sealed class InitMessage : BaseMessage, IPlayerId, IPositioned
    {
        public InitMessage() { }
        public override MessageType Type => MessageType.Init;

        public int PlayerId { get; internal set; }
        [JsonProperty("spawnPosition")]
        public Point SpawnPosition { get; internal set; }
        Point IPositioned.Position { get => SpawnPosition; }
        [JsonProperty("size")]
        public Size WorldSize { get; internal set; }
        [JsonProperty("blocks")]
        public Block[,,] Blocks { get; internal set; }
        [JsonProperty("username")]
        public string Username { get; internal set; }
        [JsonProperty("isGuest")]
        public bool IsGuest { get; internal set; }
        [JsonProperty("worldId")]
        public string WorldId { get; internal set; }
    }
    public sealed class PlayerJoinMessage : BaseMessage, IPlayerId, IPositioned
    {
        public PlayerJoinMessage() { }
        public override MessageType Type => MessageType.PlayerJoin;

        public int PlayerId { get; internal set; }
        [JsonProperty("joinLocation")]
        public Point JoinLocation { get; internal set; }
        Point IPositioned.Position { get => JoinLocation; }
        [JsonProperty("hasGun")]
        public bool HasGun { get; internal set; }
        [JsonProperty("gunEquipped")]
        public bool GunEquipped { get; internal set; }
        [JsonProperty("username")]
        public string Username { get; internal set; }
        [JsonProperty("isGuest")]
        public bool IsGuest { get; internal set; }
    }
    public sealed class PlayerLeaveMessage : BaseMessage, IPlayerId
    {
        public PlayerLeaveMessage() { }
        public override MessageType Type => MessageType.PlayerLeave;

        public int PlayerId { get; internal set; }
        public Player Player { get; internal set; }
    }
    public sealed class MovementMessage : BaseMessage, IPlayerId, IPositioned
    {
        public MovementMessage() { }
        public MovementMessage(double x, double y, bool left = false, bool right = false, bool up = false) : this(new Point(x, y), new Input(left, right, up)) { }
        public MovementMessage(Point position, bool left = false, bool right = false, bool up = false) : this(position, new Input(left, right, up)) { }
        public MovementMessage(double x, double y, Input inputs = default) : this(new Point(x, y), inputs) { }
        public MovementMessage(Point position, Input inputs = default) : this()
        {
            Position = position;
            Inputs = inputs;
            Raw = JsonConvert.SerializeObject(new { packetId, position, inputs });
        }
        public override MessageType Type => MessageType.Movement;

        public int PlayerId { get; internal set; }
        public Point Position { get; internal set; }
        [JsonProperty("inputs")]
        public Input Inputs { get; internal set; }
    }

    public sealed class PickupGunMessage : BaseMessage, IPlayerId
    {
        public PickupGunMessage() { }
        public PickupGunMessage(int x, int y) : this(new Point(x, y)) { }
        public PickupGunMessage(Point position) : this()
        {
            this.position = position;
            Raw = JsonConvert.SerializeObject(new { packetId, position });
        }
        public override MessageType Type => MessageType.PickupGun;

        internal Point position { get; set; }
        public int PlayerId { get; internal set; }
    }
    public sealed class EquipGunMessage : BaseMessage, IPlayerId
    {
        public EquipGunMessage() { }
        public EquipGunMessage(bool equipped) : this()
        {
            Equipped = equipped;
            Raw = JsonConvert.SerializeObject(new { packetId, equipped });
        }
        public override MessageType Type => MessageType.EquipGun;

        public int PlayerId { get; internal set; }
        [JsonProperty("equipped")]
        public bool Equipped { get; internal set; }
    }
    public sealed class FireBulletMessage : BaseMessage, IPlayerId
    {
        public FireBulletMessage() { }
        public FireBulletMessage(double angle) : this()
        {
            Angle = angle;
            Raw = JsonConvert.SerializeObject(new { packetId, angle });
        }
        public override MessageType Type => MessageType.FireBullet;

        public int PlayerId { get; internal set; }
        [JsonProperty("angle")]
        public double Angle { get; internal set; }
    }

    public sealed class BlockSingleMessage : BaseMessage, IPlayerId, IPositioned
    {
        public BlockSingleMessage() { }
        public BlockSingleMessage(int layer, int x, int y, int id) : this(layer, new Point(x, y), id) { }
        public BlockSingleMessage(LayerId layer, int x, int y, int id) : this((int)layer, new Point(x, y), id) { }
        public BlockSingleMessage(int layer, Point position, int id) : this()
        {
            Layer = layer;
            Position = position;
            Id = id;
            Raw = JsonConvert.SerializeObject(new { packetId, layer, position, id });
        }
        public BlockSingleMessage(LayerId layer, Point position, int id) : this((int)layer, position, id) { }
        public BlockSingleMessage(int layer, int x, int y, BlockId id) : this(layer, new Point(x, y), (int)id) { }
        public BlockSingleMessage(LayerId layer, int x, int y, BlockId id) : this((int)layer, new Point(x, y), (int)id) { }
        public BlockSingleMessage(int layer, Point position, BlockId id) : this(layer, position, (int)id) { }
        public BlockSingleMessage(LayerId layer, Point position, BlockId id) : this((int)layer, position, (int)id) { }
        public override MessageType Type => MessageType.BlockSingle;

        public int PlayerId { get; internal set; }
        public Point Position { get; internal set; }
        public int X { get => (int)Position.X; }
        public int Y { get => (int)Position.Y; }
        [JsonProperty("layer")]
        public int Layer { get; internal set; }
        [JsonProperty("id")]
        public int Id { get; internal set; }
    }
    public sealed class BlockLineMessage : BaseMessage, IPlayerId
    {
        public BlockLineMessage() { }
        public BlockLineMessage(int layer, int x1, int y1, int x2, int y2, int id) : this(layer, new Point(x1, y1), new Point(x2, y2), id) { }
        public BlockLineMessage(LayerId layer, int x1, int y1, int x2, int y2, int id) : this((int)layer, new Point(x1, y1), new Point(x2, y2), id) { }
        public BlockLineMessage(int layer, Point start, int x2, int y2, int id) : this(layer, start, new Point(x2, y2), id) { }
        public BlockLineMessage(LayerId layer, Point start, int x2, int y2, int id) : this((int)layer, start, new Point(x2, y2), id) { }
        public BlockLineMessage(int layer, int x1, int y1, Point end, int id) : this(layer, new Point(x1, y1), end, id) { }
        public BlockLineMessage(LayerId layer, int x1, int y1, Point end, int id) : this((int)layer, new Point(x1, y1), end, id) { }
        public BlockLineMessage(int layer, Point start, Point end, int id) : this()
        {
            Layer = layer;
            Start = start;
            End = end;
            Id = id;
            Raw = JsonConvert.SerializeObject(new { packetId, layer, start, end, id });
        }
        public BlockLineMessage(LayerId layer, Point start, Point end, int id) : this((int)layer, start, end, id) { }
        public BlockLineMessage(int layer, int x1, int y1, int x2, int y2, BlockId id) : this(layer, new Point(x1, y1), new Point(x2, y2), (int)id) { }
        public BlockLineMessage(LayerId layer, int x1, int y1, int x2, int y2, BlockId id) : this((int)layer, new Point(x1, y1), new Point(x2, y2), (int)id) { }
        public BlockLineMessage(int layer, Point start, int x2, int y2, BlockId id) : this(layer, start, new Point(x2, y2), (int)id) { }
        public BlockLineMessage(LayerId layer, Point start, int x2, int y2, BlockId id) : this((int)layer, start, new Point(x2, y2), (int)id) { }
        public BlockLineMessage(int layer, int x1, int y1, Point end, BlockId id) : this(layer, new Point(x1, y1), end, (int)id) { }
        public BlockLineMessage(LayerId layer, int x1, int y1, Point end, BlockId id) : this((int)layer, new Point(x1, y1), end, (int)id) { }
        public BlockLineMessage(int layer, Point start, Point end, BlockId id) : this(layer, start, end, (int)id) { }
        public BlockLineMessage(LayerId layer, Point start, Point end, BlockId id) : this((int)layer, start, end, (int)id) { }
        public override MessageType Type => MessageType.BlockLine;

        public int PlayerId { get; internal set; }
        [JsonProperty("start")]
        public Point Start { get; internal set; }
        [JsonProperty("end")]
        public Point End { get; internal set; }
        [JsonProperty("layer")]
        public int Layer { get; internal set; }
        [JsonProperty("id")]
        public int Id { get; internal set; }
    }
    public sealed class BlockBufferMessage : BaseMessage, IPlayerId
    {
        public BlockBufferMessage() { }
        public BlockBufferMessage(params BaseMessage[] msgs)
        {
            JObject json = new JObject();
            json.Add("packetId", MessageParser.EnumToPacketId(MessageType.BlockBuffer));
            JArray arr = new JArray();
            foreach (var msg in msgs)
            {
                switch (msg.Type)
                {
                    case MessageType.BlockSingle:
                    case MessageType.BlockLine:
                        arr.Add(JsonConvert.DeserializeObject(msg.Raw));
                        break;
                    default: throw new ArgumentException("message needs to be either blocksingle or blockline");
                }
            }
            Blocks = msgs;
            json.Add("blocks", arr);
            Raw = json.ToString();
        }
        public override MessageType Type => MessageType.BlockBuffer;

        public int PlayerId { get; internal set; }
        public BaseMessage[] Blocks { get; internal set; }
    }
}
