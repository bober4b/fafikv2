using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fafikv2.Commands
{
    public class Commandstest : BaseCommandModule
    {
        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("pong");
            Console.WriteLine("xDDDD");
        }
        [Command("benc")]
        public async Task Benc(CommandContext ctx, int benc1, int benc2)
        {
            await ctx.Channel.SendMessageAsync($"elo benc:{benc1+benc2*3.14} benc elo");
        }

    }
}
