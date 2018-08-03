using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using SmashOrPass.Data;

namespace SmashOrPass
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot bot = new Bot("NDc0ODc4MTM5OTA3MTc4NTA2.DkW-_g.ODCVEljWWv3TuWqBtAaQIkWUUHA");
            SmashDatabase.Init();
            bot.StartBot().GetAwaiter().GetResult();
        }
    }
}
