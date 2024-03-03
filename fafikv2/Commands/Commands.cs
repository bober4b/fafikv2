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
        public async Task ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("pong");
        }
        [Command("benc")]
        public async Task benc(CommandContext ctx, int benc1, int benc2)
        {
            await ctx.Channel.SendMessageAsync($"elo benc:{benc1+benc2*3.14} benc elo");
        }
    }
}
