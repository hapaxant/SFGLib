using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFGLib;

namespace example
{
    class Program
    {
        static void Main(string[] args)
        {
            var cli = new Client();
            var lobby = cli.LoadLobby();
            var con = cli.CreateWorldConnection("egg");
            con.OnDisconnect += (s, e) => Console.WriteLine(e);
            con.OnMessage += (s, e) =>
            {
                switch (e)
                {
                    case FireBulletMessage m:
                        Console.WriteLine(m.Angle);
                        break;
                    case InitMessage m:
                        con.SendMovement(300, 400, true, true, true);
                        break;
                }
            };
            con.Connect();
            Console.Write("egg");
            Console.ReadLine();
        }
    }
}
