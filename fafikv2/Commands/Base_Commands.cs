using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Fafikv2.Services.CommandService;
using Fafikv2.Services.dbServices.Interfaces;

namespace Fafikv2.Commands
{
    public class BaseCommands : BaseCommandModule
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

        [Command("stats")]
        public async Task Stats(Command ctx)
        {
            
        }

    }
}
