using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using WebSocketSharp;
using Newtonsoft.Json.Linq;

namespace SFGLib
{
    public class Connection : IDisposable
    {
        internal Connection(Client cli, string id)
        {
            Socket = new WebSocket(Consts.GameUrl + id) { Compression = CompressionMethod.Deflate };
            Socket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            Socket.OnMessage += (s, e) => { var m = MessageHandler.HandleMessage(e); OnMessage?.Invoke(this, m); };
            Socket.OnClose += (s, e) => OnDisconnect?.Invoke(this, $"Reason: {e.Reason}, Code: {e.Code}, WasClean: {e.WasClean}");
        }

        protected WebSocket Socket;
        public event EventHandler<BaseMessage> OnMessage;
        public event EventHandler<string> OnDisconnect;

        public Dictionary<int, Player> Players = new Dictionary<int, Player>();

        public void Connect()
        {
            Socket.Connect();
        }

        public void Dispose()
        {
            Socket?.Close();
        }


        internal void Send<T>(string packetId, T data)
        {
            var json = JObject.FromObject(data);
            json.Add("packetId", packetId);
            var txt = json.ToString();
            Socket.Send(txt);
        }
        public void SendMovement(double x = 0, double y = 0, bool left = false, bool right = false, bool up = false) => SendMovement(new Point(x, y), new Input(left, right, up));
        public void SendMovement(Point position, Input inputs)
        {
            Send(MessageHandler.EnumToPacketId(MessageType.Movement), new { position, inputs });
        }
    }
    public class Player { public int UserId { get; } public object Tag { get; set; } }
}
