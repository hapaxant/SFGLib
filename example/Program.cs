using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SFGLib;

namespace example
{
    class Program
    {
        static Client cli;
        static Connection con;
        static HashSet<int> snek = new HashSet<int>();
        static bool inited;
        static void Main(string[] args)
        {
            //var cli = Client.AuthAsGuest("test");
            cli = Client.Login("test@piss.balls", "EGG");
            var lobby = cli.LoadLobby();
            var plr = cli.LoadPlayer();
            if (lobby.Length > 0) con = cli.JoinRoom(lobby[0]);
            else con = cli.CreateDynamicRoom("WMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWM", 50, 50);
            con.OnDisconnect += (_, e) => Console.WriteLine(e);
            con.OnMessage += Con_OnMessage;

            con.Connect();
            Console.Write("egg");
            Console.ReadLine();
            Random rnd = new Random();
            double d = 0;
            double s = 0;
            con.SendBlock(LayerId.Action, 0, 3, BlockId.Gun);
            con.SendPickupGun(0, 3);
            int i = 0;
            int m1 = 1;
            int m2 = 1;
            int am = 0;
            while (con.Connected && !inited) Thread.Sleep(1);
            while (con.Connected)
            {
                d += Math.Sin(s);
                s += .1 + (Math.Sin(d) * .3);
                while (d > Math.PI) d -= Math.PI * 2;
                while (d < -Math.PI) d += Math.PI * 2;
                Console.WriteLine($"d={d}, s={s}");
                con.SendFireBullet(d);
                if (i % m2 == 0)
                {
                    m2 = rnd.Next(1, 7);
                    am = rnd.Next(1, 6);
                    for (int j = 0; j < am; j++)
                    {
                        con.SendFireBullet(d);
                    }
                }
                Thread.Sleep(64);
                i++;
                if (i % m1 == 0)
                {
                    con.SendMovement(con.Players.First(x => x.Value.PlayerId != con.ClientId).Value.Position, rnd.Next(0, 2) == 0, rnd.Next(0, 2) == 0, rnd.Next(0, 5) != 0);
                    m1 = rnd.Next(18, 34);
                }

                //con.SendBlockBuffer(new BlockSingleMessage(LayerId.Action, 7, 5, BlockId.Gun), new BlockLineMessage(LayerId.Foreground, 3, 8, 12, 8, BlockId.Solid));
                //con.SendBlockLine(LayerId.Foreground, rnd.Next(-50, 100), rnd.Next(-50, 100), rnd.Next(-50, 100), rnd.Next(-50, 100), BlockId.Solid);
            }
            Console.WriteLine("got disconnected");
            Console.ReadLine();
        }

        static bool check(int pid, int id, int l)
        {
            if (snek.Contains(pid))
                if (id == (int)BlockId.Solid && l == (int)LayerId.Foreground) return true;
            return false;
        }
        private static void Con_OnMessage(object sender, BaseMessage e)
        {
            const int delay = 600;
            if (e.Type != MessageType.Movement) Console.WriteLine(e.Type);
            switch (e)
            {
                case FireBulletMessage m:
                    Console.WriteLine(m.Angle);
                    if (snek.Contains(m.PlayerId)) snek.Remove(m.PlayerId);
                    else snek.Add(m.PlayerId);
                    break;
                case InitMessage m:
                    con.SendMovement(300, 400, true, true, true);
                    Console.WriteLine($"init #{m.PlayerId}");
                    inited = true;
                    break;
                case BlockSingleMessage m:
                    Console.WriteLine($"{m.Layer},{m.X},{m.Y},{m.Id}");
                    if (con.Players[con.ClientId].GunEquipped) break;
                    if (check(m.PlayerId, m.Id, m.Layer)) Task.Delay(delay).ContinueWith((_) => con.SendBlock(m.Layer, m.Position, BlockId.Empty));
                    break;
                case BlockLineMessage m:
                    if (con.Players[con.ClientId].GunEquipped) break;
                    if (check(m.PlayerId, m.Id, m.Layer)) Utils.BresenhamsLine(m, (x, y) =>
                    {
                        Task.Delay(delay).ContinueWith((_) =>
                        {
                            if (con.Blocks[m.Layer, x, y].Id != (int)BlockId.Solid) return;
                            con.SendBlock(m.Layer, x, y, BlockId.Empty);
                        });
                    });
                    break;
                case BlockBufferMessage m:
                    foreach (var item in m.Blocks)
                    {
                        Con_OnMessage(sender, item);
                    }
                    break;
                case PlayerJoinMessage m:
                    Console.WriteLine($"join #{m.PlayerId}");
                    break;
                case PlayerLeaveMessage m:
                    Console.WriteLine($"leave #{m.PlayerId}");
                    break;
            }
        }
    }
}
