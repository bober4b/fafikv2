using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Fafikv2.Services.CommandService;
namespace Fafikv2.Commands
{
    public class AdditionalMusicCommands : BaseCommandModule
    {
        public static AdditionalMusicService AdditionalMusicService;
        [Command("lyric")]
        public async Task Lyric(CommandContext ctx, [RemainingText] string titleAndArtist)
        {
            if (titleAndArtist.Contains('|'))
            {
                var standardisation = titleAndArtist.Split('|');
                if (standardisation.Length == 2)
                {
                    await AdditionalMusicService.FindLyric(ctx, standardisation[0], standardisation[1]) ;
                    return;
                }
            }

            await ctx.RespondAsync("zły format!!") ;
        }
    }
}
