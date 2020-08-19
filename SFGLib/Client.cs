using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace SFGLib
{
    public sealed class Client
    {
        internal Client(string token) => this.token = token;

        internal readonly string token;
        public string Id { get; internal set; }

        internal static TOut GetServerResponse<TIn, TOut>(string url, string method = "GET", TIn data = default, TOut spec = default) => JsonConvert.DeserializeAnonymousType(GetServerResponse(url, method, JsonConvert.SerializeObject(data)), default(TOut));
        internal static string GetServerResponse(string url, string method = "GET", string data = null, string token = null)
        {
            var req = WebRequest.CreateHttp(url);
            req.Method = method;

            if (token != null) req.Headers[HttpRequestHeader.Authorization] = token;

            if (data != null)
            {
                req.ContentType = "application/json";
                using (var rs = req.GetRequestStream())
                using (var sw = new StreamWriter(rs))
                    sw.Write(data);
            }

            using (var wr = req.GetResponse())
            using (var rs = wr.GetResponseStream())
            using (var sr = new StreamReader(rs))
                return sr.ReadToEnd();
        }

        public static Client AuthAsGuest(string username = "guest") => new Client(GetServerResponse(Consts.GuestAuthUrl, "POST", new { username }, new { token = default(string) }).token);
        public static Client Register(string username, string email, string password)
        {
            var resp = GetServerResponse(Consts.RegisterUrl, "POST", new { username, email, password }, new { token = default(string), id = default(string) });
            return new Client(resp.token) { Id = resp.id };
        }
        public static Client Login(string email, string password)
        {
            var resp = GetServerResponse(Consts.LoginUrl, "POST", new { email, password }, new { token = default(string), id = default(string) });
            return new Client(resp.token) { Id = resp.id };
        }

        public LobbyRoom[] LoadLobby() => JsonConvert.DeserializeObject<LobbyRoom[]>(GetServerResponse(Consts.LobbyUrl, token: token));

        public PlayerInfo LoadPlayer()
        {//hacky code, this is temp anyway
            var r = GetServerResponse(Consts.PlayerUrl, token: token);
            var o = (dynamic)JsonConvert.DeserializeObject(r);
            bool isguest = false;
            string name = null;
            LobbyRoom[] rooms = null;
            try
            {
                isguest = o.isGuest;
                name = o.name;
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
            {
                rooms = JsonConvert.DeserializeObject<LobbyRoom[]>(r);
            }

            return new PlayerInfo(rooms ?? new LobbyRoom[0], name, isguest);
        }

        public Connection JoinRoom(LobbyRoom room) => JoinRoom(room.Id, room.Type);
        public Connection JoinRoom(string id, RoomType type) => Connection.CreateConnection(this, new { id, type = type.ToString().ToLowerInvariant() });
        public Connection CreateDynamicRoom(string name, Size size) => CreateDynamicRoom(name, (int)size.Width, (int)size.Height);
        public Connection CreateDynamicRoom(string name, int width, int height) => Connection.CreateConnection(this, new { type = "dynamic", name, width, height });
    }
}
