using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Fafikv2.Commands.MessageCreator;
using Fafikv2.Services.CommandService;
namespace Fafikv2.Commands
{
    public class AdditionalMusicCommands : BaseCommandModule
    {
        private readonly AdditionalMusicService _service;


        public AdditionalMusicCommands(AdditionalMusicService service)
        {
            _service=service;
        }

        [Command("lyric")]
        public async Task Lyric(CommandContext ctx, [RemainingText] string titleAndArtist)
        {
            if (titleAndArtist.Contains('|'))
            {
                var standardisation = titleAndArtist.Split('|');
                if (standardisation.Length == 2)
                {
                    await _service.FindLyric(ctx, standardisation[0], standardisation[1]);
                    return;
                }
            }

            await ctx.RespondAsync(MessagesComposition.EmbedMessageComposition("lyric","incorrect format"));
        }

        [Command("Help")]

        public async Task Help(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Command List",
                Description = "Here are all the available commands you can use:",
                Color = DiscordColor.Blurple
            };

            embed.AddField("🎶 Music",
                "**Join** - Joins the specified channel\n" +
                "**Leave** - Leaves the current channel\n" +
                "**Play** - Plays a song based on your search\n" +
                "**Pause** - Pauses the currently playing song\n" +
                "**Resume** - Resumes song playback\n" +
                "**Skip** - Skips to the next song\n" +
                "**Queue** - Displays the song queue\n" +
                "**Volume** - Sets playback volume\n" +
                "**AutoPlay** - Enables automatic music playback\n" +
                "**AutoPlayByGenre** - Automatically plays music by genre");

            embed.AddField("🛡️ Moderation",
                "**kick_enable** - Enables kicks on the server\n" +
                "**kick_disable** - Disables kicks on the server\n" +
                "**ban_enable** - Enables bans on the server\n" +
                "**ban_disable** - Disables bans on the server\n" +
                "**auto_moderator_enable** - Enables automatic moderator\n" +
                "**auto_moderator_disable** - Disables automatic moderator\n" +
                "**banned** - Adds a banned word\n" +
                "**rbanned** - Removes a banned word\n"+
                "**auto_play_enable** - Enables auto play queue on the server\n" + 
                "**auto_play_disable** - Disables auto play queue on the server");

            embed.AddField("📈 Statistics",
                "**stats** - Displays your statistics\n" +
                "**leaderboard** - Shows the top 3 users on the server");

            await ctx.RespondAsync(embed: embed);
        }

    }
}
