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
            Socket.OnMessage += (s, e) => OnMessage?.Invoke(this, MessageHandler.HandleMessage(e));
            Socket.OnClose += (s, e) => OnDisconnect?.Invoke(this, $"Reason: {e.Reason}, Code: {e.Code}, WasClean: {e.WasClean}");
        }

        protected WebSocket Socket;
        public event EventHandler<BaseMessage> OnMessage;
        public event EventHandler<string> OnDisconnect;

        public void Connect()
        {
            Socket.Connect();
        }

        public void Dispose()
        {
            Socket?.Close();
        }
    }
}
