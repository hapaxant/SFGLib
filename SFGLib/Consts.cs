namespace SFGLib
{
    internal static class Consts
    {
        internal const string BaseApiUrl = "sirjosh3917.com/smiley-face-game/v1/";
        internal const string GuestAuthUrl = "https://beta-api." + BaseApiUrl + "auth/guest";
        internal const string LoginUrl = "https://beta-api." + BaseApiUrl + "auth/login";
        internal const string RegisterUrl = "https://beta-api." + BaseApiUrl + "auth/register";
        internal const string LobbyUrl = "https://beta-api." + BaseApiUrl + "game/lobby";
        internal const string PlayerUrl = "https://beta-api." + BaseApiUrl + "player";
        internal const string BaseGameUrl = "wss://beta-ws-api." + BaseApiUrl + "game/ws/?";
    }
}
