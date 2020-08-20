using Newtonsoft.Json;
using System;

namespace SFGLib
{
    public struct PlayerInfo
    {
        [JsonConstructor]
        internal PlayerInfo(LobbyRoom[] ownedWorlds, string name, bool isGuest, int energy, int maxEnergy, int energyRegenerationRateMs, int lastEnergyAmount, ulong timeEnergyWasAtAmount)
        {
            OwnedWorlds = ownedWorlds?? new LobbyRoom[0];
            Name = name;
            IsGuest = isGuest;
            Energy = energy;
            MaxEnergy = maxEnergy;
            EnergyRegenerationRate = energyRegenerationRateMs;
            LastEnergyAmount = lastEnergyAmount;
            TimeEnergyWasAtAmount = timeEnergyWasAtAmount;
        }

        public string Name { get; }
        public bool IsGuest { get; }
        public int Energy { get; }
        public int MaxEnergy { get; }
        public int EnergyRegenerationRate { get; }
        public int LastEnergyAmount { get; }
        public ulong TimeEnergyWasAtAmount { get; }
        public LobbyRoom[] OwnedWorlds { get; }
    }

    public struct LobbyRoom
    {
        [JsonConstructor]
        internal LobbyRoom(string id, int playerCount, string type, string name)
        {
            Id = id;
            PlayerCount = playerCount;
            this._type = type;
            Name = name;
        }

        public string Id { get; }
        public int PlayerCount { get; }
        internal string _type { get; }
        public RoomType Type { get => (RoomType)Enum.Parse(typeof(RoomType), _type, true); }
        public string Name { get; }
    }

    public enum RoomType
    {
        Saved, Dynamic
    }

    public enum MessageType
    {
        Unknown, Init, PlayerJoin, PlayerLeave, Movement, PickupGun, EquipGun, FireBullet, BlockSingle, BlockLine, BlockBuffer
    }

    public struct Point
    {
        public Point(double x, double y) { X = x; Y = y; }

        [JsonProperty("x")]
        public double X { get; }
        [JsonProperty("y")]
        public double Y { get; }
    }
    public struct Size
    {
        public Size(double width, double height) { Width = width; Height = height; }

        [JsonProperty("width")]
        public double Width { get; }
        [JsonProperty("height")]
        public double Height { get; }
    }

    public enum BlockId
    {
        Empty = 0, Solid = 1, Gun = 2
    }
    public enum LayerId
    {
        Foreground = 0, Action = 1, Background = 2
    }

    public struct Block
    {
        [JsonConstructor]
        public Block(int id) { Id = id; UserId = 0; }
        public Block(int id, int userId) { Id = id; UserId = userId; }

        [JsonProperty("id")]
        public int Id { get; }
        public int UserId { get; internal set; }
    }

    public struct Input
    {
        public Input(bool left, bool right, bool up) { Left = left; Right = right; Up = up; }

        [JsonProperty("left")]
        public bool Left { get; internal set; }
        [JsonProperty("right")]
        public bool Right { get; internal set; }
        [JsonProperty("up")]
        public bool Up { get; internal set; }
    }

    public class Player : IPlayerId
    {
        public Player(int playerId) => PlayerId = playerId;

        public int PlayerId { get; }
        public string Username { get; internal set; }
        public bool IsGuest { get; internal set; }
        public Point Position { get; internal set; }
        public double X { get => Position.X; }
        public double Y { get => Position.Y; }
        public Input Inputs { get; internal set; }
        public bool GunEquipped { get; internal set; }
        public bool HasGun { get; internal set; }
        public double GunAngle { get; internal set; }
        public object Tag { get; set; }
    }
}
