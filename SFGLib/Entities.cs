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
        Init, PlayerJoin, Movement, BlockSingle
    }

    public struct Point
    {
        public Point(int x, int y) { X = x; Y = y; }
        [JsonProperty("x")]
        public int X { get; }
        [JsonProperty("y")]
        public int Y { get; }
    }
    public struct Size
    {
        public Size(int width, int height) { Width = width; Height = height; }
        [JsonProperty("width")]
        public int Width { get; }
        [JsonProperty("height")]
        public int Height { get; }
    }

    public struct Block
    {
        public Block(int id) { Id = id; }
        [JsonProperty("id")]
        public int Id { get; }
    }

}
