namespace SFGLib
{
    public struct LobbyWorld
    {
        public LobbyWorld(string id, int playerCount) { this.Id = id; this.PlayerCount = playerCount; }

        public string Id { get; }
        public int PlayerCount { get; }
    }
}
