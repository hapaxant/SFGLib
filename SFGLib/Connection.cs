using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using WebSocketSharp;

namespace SFGLib
{
    public class Connection : IDisposable
    {
        internal Connection(Client cli)
        {

        }

        WebSocket Socket;

        public void Dispose()
        {
            Socket?.Close();
        }
    }
}
