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
            con.OnMessage += (s, m) =>
            {

            };
            con.Connect();
            Console.Write("egg");
            Console.ReadLine();
        }
    }
}
