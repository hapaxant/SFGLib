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
            //var con = cli.JoinRoom(plr.OwnedRooms[0]);
            var con = cli.CreateDynamicRoom("WMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWMWM", 50, 7);
            con.OnDisconnect += (s, e) => Console.WriteLine(e);
            HashSet<int> optedIn = new HashSet<int>();
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
                        if (optedIn.Contains(m.PlayerId))
                            if (m.Id == (int)BlockId.Solid && m.Layer == (int)LayerId.Foreground)
                                Task.Delay(200).ContinueWith((_) => con.SendBlock(m.Layer, m.Position, BlockId.Empty));
                        break;
                    case PlayerJoinMessage m:
                        Console.WriteLine($"join #{m.PlayerId}");
                        break;
                }
            };
            con.Connect();
            Console.Write("egg");
            Console.ReadLine();
            Random rnd = new Random();
            while (con.Connected)
            {
                Console.ReadLine();
            }
        }
    }
}
