using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Fafikv2.Services.CommandService;

namespace Fafikv2.Commands
{
    public class MusicCommands : BaseCommandModule
    {
        public static MusicService _musicService;
        

        [Command]
        public async Task Join(CommandContext ctx, DiscordChannel? channel=null)
        {

            await _musicService.JoinAsync(ctx, channel);

        }

        [Command]
        public async Task Leave(CommandContext ctx)
        {
            await MusicService.LeaveAsync(ctx);
        }

        [Command]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            await _musicService.PlayAsync(ctx, search);
        }

        [Command]
        public async Task Pause(CommandContext ctx)
        {
            await MusicService.PauseAsync(ctx).ConfigureAwait(false);
        }

        [Command]
        public async Task Resume(CommandContext ctx)
        {
            await MusicService.ResumeAsync(ctx);
        }

        [Command]
        public async Task Skip(CommandContext ctx)
        {
            await _musicService.SkipAsync(ctx);
        }

        [Command]
        public async Task Queue(CommandContext ctx)
        {
            await _musicService.QueueAsync(ctx);
        }

        [Command]
        public async Task Volume(CommandContext ctx, int vol)
        {
            await MusicService.VolumeAsync(ctx, vol);
        }

        [Command("AutoPlay")]
        public async Task AutoPlay(CommandContext ctx)
        {
            _musicService.StartAutoplay(ctx);
        }
        [Command("AutoPlayByGenre")]
        public async Task AutoPlayByGenre(CommandContext ctx, [RemainingText] string genre)
        {
            _musicService.StartAutoplay(ctx);
        }
    }
}
