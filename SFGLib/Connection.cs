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
    public sealed class Connection : IDisposable
    {
        internal Connection() { }

        internal static Connection CreateConnection<T>(Client cli, T args)
        {
            var con = new Connection();
            try
            {
                con.Client = cli;

                string url;
                using (var form = new System.Net.Http.FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("token", con.Client.token),
                    new KeyValuePair<string, string>("world", JsonConvert.SerializeObject(args))
                }))
                    url = Consts.BaseGameUrl + form.ReadAsStringAsync().Result;

                con.Socket = new WebSocket(url) { Compression = CompressionMethod.Deflate };
                con.Socket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                con.Socket.OnMessage += (s, e) => { var m = MessageParser.ParseMessage(e); con.HandleMessage(m); con.OnMessage?.Invoke(con, m); };
                con.Socket.OnClose += (s, e) => con.OnDisconnect?.Invoke(con, $"Reason: {e.Reason}, Code: {e.Code}, WasClean: {e.WasClean}");
                return con;
            }
            catch
            {
                con?.Dispose();
                throw;
            }
        }

        internal WebSocket Socket;
        internal Client Client;
        public event EventHandler<BaseMessage> OnMessage;
        public event EventHandler<string> OnDisconnect;
        public bool Connected { get => Socket?.ReadyState == WebSocketState.Open; }

        public int ClientId { get; internal set; }
        public Size WorldSize { get; internal set; }
        public Block[,,] Blocks { get; internal set; }
        internal Dictionary<int, Player> _players = new Dictionary<int, Player>();
        public IReadOnlyDictionary<int, Player> Players { get => _players; }

        internal void HandleMessage(BaseMessage e)
        {
            switch (e)
            {
                case InitMessage m:
                    {
                        WorldSize = m.WorldSize;
                        Blocks = m.Blocks;
                        ClientId = m.PlayerId;
                        _players.Add(m.PlayerId, new Player(m.PlayerId) { Position = m.SpawnPosition });
                        break;
                    }
                case PlayerJoinMessage m:
                    {
                        _players.Add(m.PlayerId, new Player(m.PlayerId) { Position = m.JoinLocation, HasGun = m.HasGun, GunEquipped = m.GunEquipped });
                        break;
                    }
                case PlayerLeaveMessage m:
                    {
                        m.Player = _players[m.PlayerId];
                        _players.Remove(m.PlayerId);
                        break;
                    }
                case BlockSingleMessage m:
                    {
                        Blocks[m.Layer, m.X, m.Y] = new Block(m.Id, m.PlayerId);
                        break;
                    }
                case MovementMessage m:
                    {
                        var p = _players[m.PlayerId];
                        p.Position = m.Position;
                        p.Inputs = m.Inputs;
                        break;
                    }
                case PickupGunMessage m:
                    {
                        var p = _players[m.PlayerId];
                        //p.HasGun = p.GunEquipped = true;
                        p.HasGun = true;
                        break;
                    }
                case EquipGunMessage m:
                    {
                        _players[m.PlayerId].GunEquipped = m.Equipped;
                        break;
                    }
                case FireBulletMessage m: break;
                default:
                    Console.WriteLine($"unhandled {e.Type}!");
                    break;
            }
        }

        public void Connect()
        {
            Socket.Connect();
        }

        public void Dispose()
        {
            Socket?.Close();
        }

        public void SendRaw(string json) => Socket.Send(json);
        public void Send<T>(MessageType type, T data) => Send(MessageParser.EnumToPacketId(type), data);
        public void Send<T>(string packetId, T data)
        {
            var json = JObject.FromObject(data);
            json.Add("packetId", packetId);
            SendRaw(json.ToString());
        }

        public void SendMovement(double x, double y, bool left = false, bool right = false, bool up = false) => SendMovement(new Point(x, y), new Input(left, right, up));
        public void SendMovement(Point position, bool left = false, bool right = false, bool up = false) => SendMovement(position, new Input(left, right, up));
        public void SendMovement(double x, double y, Input inputs = default) => SendMovement(new Point(x, y), inputs);
        public void SendMovement(Point position, Input inputs = default) => Send(MessageType.Movement, new { position, inputs });

        public void SendBlock(int layer, int x, int y, int id) => SendBlock(layer, new Point(x, y), id);
        public void SendBlock(LayerId layer, int x, int y, int id) => SendBlock((int)layer, new Point(x, y), id);
        public void SendBlock(int layer, Point position, int id) => Send(MessageType.BlockSingle, new { layer, position, id });
        public void SendBlock(LayerId layer, Point position, int id) => SendBlock((int)layer, position, id);
        public void SendBlock(int layer, int x, int y, BlockId id) => SendBlock(layer, new Point(x, y), (int)id);
        public void SendBlock(LayerId layer, int x, int y, BlockId id) => SendBlock((int)layer, new Point(x, y), (int)id);
        public void SendBlock(int layer, Point position, BlockId id) => SendBlock(layer, position, (int)id);
        public void SendBlock(LayerId layer, Point position, BlockId id) => SendBlock((int)layer, position, (int)id);

        public void SendPickupGun(int x, int y) => SendPickupGun(new Point(x, y));
        public void SendPickupGun(Point position) => Send(MessageType.PickupGun, new { position });

        public void SendEquipGun(bool equipped) => Send(MessageType.EquipGun, new { equipped });

        public void SendFireBullet(double angle) => Send(MessageType.FireBullet, new { angle });
    }
}
