using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Fafikv2.Services.CommandService;


namespace Fafikv2.Commands
{
    public class BaseCommands : BaseCommandModule
    {
        public static BaseCommandService? BaseCommandService; //do poprawy w przyszłości


        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            string songTitle = "Super Song";
            TimeSpan duration = TimeSpan.FromSeconds(180); // 3 minutes song
            int totalBars = 20; // Number of bars in the progress bar

            var embed = new DiscordEmbedBuilder
            {
                Title = songTitle,
                Description = "Odtwarzanie piosenki...",
                Color = DiscordColor.Purple
            };

            var message = await ctx.Channel.SendMessageAsync(embed) ;
            int j = 0;
            for (int i = 0; i <= 180; i++)
            {
                var currentTime = TimeSpan.FromSeconds(i);
                if (i!=0 && i%(180/totalBars)==0)
                {
                    j++;
                }
                string progressBar = CreateProgressBar(j, totalBars);
                embed.Description = $"`[{progressBar}] {currentTime:mm\\:ss} / {duration:mm\\:ss}`";

                await message.ModifyAsync(embed: new Optional<DiscordEmbed>(embed)) ;
                await Task.Delay(1000 ) ; // Delay proportional to song duration
            }

            embed.Description = "Piosenka zakończona!";
            await message.ModifyAsync(embed: new Optional<DiscordEmbed>(embed)) ;
        }

        private string CreateProgressBar(int progress, int total)
        {
            int completedBars = progress;
            int remainingBars = total - progress;

            string bar = new string('█', completedBars) + new string('─', remainingBars);
            return bar;
        }
        [Command("benc")]
        public async Task Benc(CommandContext ctx, int benc1, int benc2)
        {
            await ctx.Channel.SendMessageAsync($"elo benc:{benc1+benc2*3.14} benc elo") ;
        }

        [Command("stats")]
        public async Task Stats(CommandContext ctx)
        {
            await BaseCommandService!.Stats(ctx) ;
        }

        [Command("leaderboard")]
        public async Task Leaderboard(CommandContext ctx)
        {
            await BaseCommandService!.Leaderboard(ctx) ;
        }

    }
}
