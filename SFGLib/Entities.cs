using Newtonsoft.Json;

namespace SFGLib
{
    public struct LobbyWorld
    {
        public LobbyWorld(string id, int playerCount) { this.Id = id; this.PlayerCount = playerCount; }

        public string Id { get; }
        public int PlayerCount { get; }
    }

    public enum MessageType
    {
        Unknown, Init, PlayerJoin, PlayerLeave, Movement, PickupGun, EquipGun, FireBullet, BlockSingle,
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
}
