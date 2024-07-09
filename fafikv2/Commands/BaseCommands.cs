using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Fafikv2.Services.CommandService;


namespace Fafikv2.Commands
{
    public class BaseCommands : BaseCommandModule
    {
        public static BaseCommandService? BaseCommandService; //do poprawy w przyszłości


        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("pong").ConfigureAwait(false);
            Console.WriteLine("xDDDD");
        }
        [Command("benc")]
        public async Task Benc(CommandContext ctx, int benc1, int benc2)
        {
            await ctx.Channel.SendMessageAsync($"elo benc:{benc1+benc2*3.14} benc elo").ConfigureAwait(false);
        }

        [Command("stats")]
        public async Task Stats(CommandContext ctx)
        {
            await BaseCommandService!.Stats(ctx).ConfigureAwait(false);
        }

        [Command("leaderboard")]
        public async Task Leaderboard(CommandContext ctx)
        {
            await BaseCommandService!.Leaderboard(ctx).ConfigureAwait(false);
        }

    }
}
