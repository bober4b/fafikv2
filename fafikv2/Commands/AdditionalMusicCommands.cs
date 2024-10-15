using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Fafikv2.Services.CommandService;
namespace Fafikv2.Commands
{
    public class AdditionalMusicCommands : BaseCommandModule
    {
        public static AdditionalMusicService Service { get; set; } = null!;

        [Command("lyric")]
        public async Task Lyric(CommandContext ctx, [RemainingText] string titleAndArtist)
        {
            if (titleAndArtist.Contains('|'))
            {
                var standardisation = titleAndArtist.Split('|');
                if (standardisation.Length == 2)
                {
                    await Service.FindLyric(ctx, standardisation[0], standardisation[1]) ;
                    return;
                }
            }

            await ctx.RespondAsync("zły format!!") ;
        }

        [Command("Help")]

        public Task Help(CommandContext ctx)
        {
            ctx.RespondAsync(
                "lyric-słowa piosenki" +
                "\r\nbanned-dodaj zakazane słowo" +
                "\r\nrbanned- usuń zakazane słowo" +
                "\r\nkick_enable- Włącz kicki na serwerze" +
                "\r\nkick_disable- wyłącz kicki na serwerze" +
                "\r\nban_enable- włącz bany na serwerze" +
                "\r\nban_disabled- wyłącz bany na serwerze" +
                "\r\nauto_moderator_enable-włącz automatycznego moderatora" +
                "\r\nauto_moderator_disable- wyłącz automatycznego moderatora" +
                "\r\nauto_play_enable- włącz auto kolejkę na serwerze" +
                "\r\nauto_play_disable- wyłącz auto kolejkę na serwerze" +
                "\r\nstats- wyświetl swoje statystyki" +
                "\r\nleaderboard- pokarz top 3 osoby na serwerze" +
                "\r\nJoin: Dołącza do wybranego kanału." +
                "\r\nLeave: Opuść aktualny kanał." +
                "\r\nPlay: Odtwarza utwór na podstawie wyszukiwania." +
                "\r\nPause: Pauzuje aktualnie odtwarzany utwór." +
                "\r\nResume: Wznawia odtwarzanie utworu." +
                "\r\nSkip: Przechodzi do następnego utworu." +
                "\r\nQueue: Wyświetla listę odtwarzanych utworów." +
                "\r\nVolume: Ustawia głośność odtwarzania." +
                "\r\nAutoPlay: Automatycznie odtwarza muzykę." +
                "\r\nAutoPlayByGenre: Automatyczne odtwarzanie według gatunku.");
            return Task.CompletedTask;
        }
    }
}
