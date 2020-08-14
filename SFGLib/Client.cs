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
    public class Client
    {
        public Client()
        {

        }

        public LobbyWorld[] LoadLobby()
        {
            using (var wr = WebRequest.CreateHttp(Consts.LobbyUrl).GetResponse())
            using (var rs = wr.GetResponseStream())
            using (var sr = new StreamReader(rs))
                return JsonConvert.DeserializeObject<LobbyWorld[]>(sr.ReadToEnd());
        }

        public Connection CreateWorldConnection(string id) => new Connection(this, id);
    }
}
