using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace SmashOrPass.Modules
{
    public class Ping : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task PingAsync()
        {
            await Context.Channel.SendMessageAsync("Pong");
        }
    }
}
