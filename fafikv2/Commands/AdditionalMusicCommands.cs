using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Fafikv2.Services.CommandService;
namespace Fafikv2.Commands
{
    public class AdditionalMusicCommands : BaseCommandModule
    {
        public static AdditionalMusicService AdditionalMusicService;
        [Command("lyric")]
        public async Task Lyric(CommandContext ctx,string title, string artist)
        {
            await AdditionalMusicService.FindLyric(ctx,title, artist).ConfigureAwait(false);
        }
    }
}
