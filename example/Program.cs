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
        static void Main(string[] args)
        {
            //var cli = Client.AuthAsGuest("test");
            var cli = Client.Login("test@piss.balls", "EGG");
            var lobby = cli.LoadLobby();
            var plr = cli.LoadPlayer();
            Connection con;
            if (lobby.Length > 0) con = cli.JoinRoom(lobby[0]);
            else con = cli.CreateDynamicRoom("WMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWM", 50, 50);
            con.OnDisconnect += (s, e) => Console.WriteLine(e);
            HashSet<int> optedIn = new HashSet<int>();
            bool check(int pid, int id, int l)
            {
                if (optedIn.Contains(pid))
                    if (id == (int)BlockId.Solid && l == (int)LayerId.Foreground) return true;
                return false;
            }
            con.OnMessage += (s, e) =>
            {
                if (e.Type != MessageType.Movement) Console.WriteLine(e.Type);
                switch (e)
                {
                    case FireBulletMessage m:
                        Console.WriteLine(m.Angle);
                        if (optedIn.Contains(m.PlayerId)) optedIn.Remove(m.PlayerId);
                        else optedIn.Add(m.PlayerId);
                        break;
                    case InitMessage m:
                        con.SendMovement(300, 400, true, true, true);
                        Console.WriteLine($"init #{m.PlayerId}");
                        break;
                    case BlockSingleMessage m:
                        Console.WriteLine($"{m.Layer},{m.X},{m.Y},{m.Id}");
                        if (check(m.PlayerId, m.Id, m.Layer)) Task.Delay(200).ContinueWith((_) => con.SendBlock(m.Layer, m.Position, BlockId.Empty));
                        break;
                    case BlockLineMessage m:
                        if (check(m.PlayerId, m.Id, m.Layer)) Utils.BresenhamsLine(m, (x, y) => Task.Delay(200).ContinueWith((_) => con.SendBlockLine(m.Layer, m.Start, m.End, BlockId.Empty)));
                        break;
                    case PlayerJoinMessage m:
                        Console.WriteLine($"join #{m.PlayerId}");
                        break;
                    case PlayerLeaveMessage m:
                        Console.WriteLine($"leave #{m.PlayerId}");
                        break;
                }
            };
            con.Connect();
            Console.Write("egg");
            //Console.ReadLine();
            //Random rnd = new Random();
            while (con.Connected)
            {
                //Thread.Sleep(200);
                Console.ReadLine();
                con.SendBlockBuffer(new BlockSingleMessage(LayerId.Action, 7, 5, BlockId.Gun), new BlockLineMessage(LayerId.Foreground, 3, 8, 12, 8, BlockId.Solid));
                //con.SendBlockLine(LayerId.Foreground, rnd.Next(-50, 100), rnd.Next(-50, 100), rnd.Next(-50, 100), rnd.Next(-50, 100), BlockId.Solid);
            }
        }
    }
}
