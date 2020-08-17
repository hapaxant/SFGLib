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
            Client = cli;
            Socket = new WebSocket(Consts.GameUrl + id) { Compression = CompressionMethod.Deflate };
            Socket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            Socket.OnMessage += (s, e) => { var m = MessageParser.ParseMessage(e); HandleMessage(m); OnMessage?.Invoke(this, m); };
            Socket.OnClose += (s, e) => OnDisconnect?.Invoke(this, $"Reason: {e.Reason}, Code: {e.Code}, WasClean: {e.WasClean}");
        }

        protected WebSocket Socket;
        protected Client Client;
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
                        ClientId = m.UserId;
                        _players.Add(m.UserId, new Player(m.UserId) { Position = m.SpawnPosition });
                        break;
                    }
                case PlayerJoinMessage m:
                    {
                        _players.Add(m.UserId, new Player(m.UserId) { Position = m.JoinLocation, HasGun = m.HasGun, GunEquipped = m.GunEquipped });
                        break;
                    }
                case PlayerLeaveMessage m:
                    {
                        m.Player = _players[m.UserId];
                        _players.Remove(m.UserId);
                        break;
                    }
                case BlockSingleMessage m:
                    {
                        Blocks[m.Layer, m.X, m.Y] = new Block(m.Id, m.UserId);
                        break;
                    }
                case MovementMessage m:
                    {
                        var p = _players[m.UserId];
                        p.Position = m.Position;
                        p.Inputs = m.Inputs;
                        break;
                    }
                case PickupGunMessage m:
                    {
                        var p = _players[m.UserId];
                        p.HasGun = p.GunEquipped = true;
                        break;
                    }
                case EquipGunMessage m:
                    {
                        _players[m.UserId].GunEquipped = m.Equipped;
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
    public class Player
    {
        public Player(int userId) => UserId = userId;

        public int UserId { get; }
        public Point Position { get; internal set; }
        public double X { get => Position.X; }
        public double Y { get => Position.Y; }
        public Input Inputs { get; internal set; }
        public bool GunEquipped { get; internal set; }
        public bool HasGun { get; internal set; }
        public object Tag { get; set; }
    }
}
