using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Fafikv2.Services.CommandService;

namespace Fafikv2.Commands
{
    public class MusicCommands : BaseCommandModule
    {
        public static MusicService? MusicService;
        

        [Command]
        public async Task Join(CommandContext ctx, DiscordChannel? channel=null)
        {

            await MusicService.JoinAsync(ctx, channel).ConfigureAwait(false);

        }

        [Command]
        public async Task Leave(CommandContext ctx)
        {
            await MusicService.LeaveAsync(ctx).ConfigureAwait(false);
        }

        [Command]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            await MusicService.PlayAsync(ctx, search).ConfigureAwait(false);
        }

        [Command]
        public async Task Pause(CommandContext ctx)
        {
            await MusicService.PauseAsync(ctx).ConfigureAwait(false);
        }

        [Command]
        public async Task Resume(CommandContext ctx)
        {
            await MusicService.ResumeAsync(ctx).ConfigureAwait(false);
        }

        [Command]
        public async Task Skip(CommandContext ctx)
        {
            await MusicService.SkipAsync(ctx).ConfigureAwait(false);
        }

        [Command]
        public async Task Queue(CommandContext ctx)
        {
            await MusicService.QueueAsync(ctx);
        }

        [Command]
        public async Task Volume(CommandContext ctx, int vol)
        {
            await MusicService.VolumeAsync(ctx, vol);
        }

        [Command("AutoPlay")]
        public async Task AutoPlay(CommandContext ctx)
        {
            await MusicService.StartAutoplay(ctx).ConfigureAwait(false);
        }
        [Command("AutoPlayByGenre")]
        public async Task AutoPlayByGenre(CommandContext ctx, [RemainingText] string genre)
        {
            await MusicService.StartAutoPlayByGenre(ctx,genre).ConfigureAwait(false);
        }
    }
}
