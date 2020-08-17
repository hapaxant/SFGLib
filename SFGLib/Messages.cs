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

        internal static BaseMessage ParseMessage(MessageEventArgs m)
        {
            var raw = m.Data;
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
                case MessageType.PlayerJoin:
                    {
                        msg = CreateMessage<PlayerJoinMessage>(raw);
                        break;
                    }
                case MessageType.PlayerLeave:
                    {
                        msg = CreateMessage<PlayerLeaveMessage>(raw);
                        break;
                    }
                case MessageType.Movement:
                    {
                        msg = CreateMessage<MovementMessage>(raw);
                        break;
                    }
                case MessageType.BlockSingle:
                    {
                        msg = CreateMessage<BlockSingleMessage>(raw);
                        break;
                    }
                case MessageType.FireBullet:
                    {
                        msg = CreateMessage<FireBulletMessage>(raw);
                        break;
                    }
                case MessageType.PickupGun:
                    {
                        msg = CreateMessage<PickupGunMessage>(raw);
                        break;
                    }
                case MessageType.EquipGun:
                    {
                        msg = CreateMessage<EquipGunMessage>(raw);
                        break;
                    }
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

    public interface IUserId {[JsonProperty("userId")] int UserId { get; } }

    public abstract class BaseMessage
    {
        internal BaseMessage() { }
        internal BaseMessage(MessageType type) { Type = type; }

        public MessageType Type { get; internal set; }
        public string Raw { get; internal set; }
    }

    public sealed class InitMessage : BaseMessage, IUserId
    {
        public InitMessage() : base(MessageType.Init) { }

        [JsonProperty("sender")]
        public int UserId { get; internal set; }
        [JsonProperty("spawnPosition")]
        public Point SpawnPosition { get; internal set; }
        [JsonProperty("size")]
        public Size WorldSize { get; internal set; }
        [JsonProperty("blocks")]
        public Block[,,] Blocks { get; internal set; }
    }
    public sealed class PlayerJoinMessage : BaseMessage, IUserId
    {
        public PlayerJoinMessage() : base(MessageType.PlayerJoin) { }

        public int UserId { get; internal set; }
        [JsonProperty("joinLocation")]
        public Point JoinLocation { get; internal set; }
        [JsonProperty("hasGun")]
        public bool HasGun { get; internal set; }
        [JsonProperty("gunEquipped")]
        public bool GunEquipped { get; internal set; }
    }
    public sealed class PlayerLeaveMessage : BaseMessage, IUserId
    {
        public PlayerLeaveMessage() : base(MessageType.PlayerLeave) { }

        public int UserId { get; internal set; }
        public Player Player { get; internal set; }
    }
    public sealed class MovementMessage : BaseMessage, IUserId
    {
        public MovementMessage() : base(MessageType.Movement) { }

        [JsonProperty("sender")]
        public int UserId { get; internal set; }
        [JsonProperty("position")]
        public Point Position { get; internal set; }
        [JsonProperty("inputs")]
        public Input Inputs { get; internal set; }
    }

    public sealed class PickupGunMessage : BaseMessage, IUserId
    {
        public PickupGunMessage() : base(MessageType.PickupGun) { }

        [JsonProperty("sender")]
        public int UserId { get; internal set; }
    }
    public sealed class EquipGunMessage : BaseMessage, IUserId
    {
        public EquipGunMessage() : base(MessageType.EquipGun) { }

        [JsonProperty("sender")]
        public int UserId { get; internal set; }
        [JsonProperty("equipped")]
        public bool Equipped { get; internal set; }
    }
    public sealed class FireBulletMessage : BaseMessage, IUserId
    {
        public FireBulletMessage() : base(MessageType.FireBullet) { }

        [JsonProperty("sender")]
        public int UserId { get; internal set; }
        [JsonProperty("angle")]
        public double Angle { get; internal set; }
    }

    public sealed class BlockSingleMessage : BaseMessage, IUserId
    {
        public BlockSingleMessage() : base(MessageType.BlockSingle) { }

        [JsonProperty("sender")]
        public int UserId { get; internal set; }
        [JsonProperty("position")]
        public Point Position { get; internal set; }
        public int X { get => (int)Position.X; }
        public int Y { get => (int)Position.Y; }
        [JsonProperty("layer")]
        public int Layer { get; internal set; }
        [JsonProperty("id")]
        public int Id { get; internal set; }
    }
}
