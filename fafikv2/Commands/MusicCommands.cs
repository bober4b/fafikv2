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

            await MusicService.JoinAsync(ctx, channel) ;

        }

        [Command]
        public async Task Leave(CommandContext ctx)
        {
            await MusicService.LeaveAsync(ctx) ;
        }

        [Command]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            await MusicService.PlayAsync(ctx, search) ;
        }

        [Command]
        public async Task Pause(CommandContext ctx)
        {
            await MusicService.PauseAsync(ctx) ;
        }

        [Command]
        public async Task Resume(CommandContext ctx)
        {
            await MusicService.ResumeAsync(ctx) ;
        }

        [Command]
        public async Task Skip(CommandContext ctx)
        {
            await MusicService.SkipAsync(ctx) ;
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
            await MusicService.StartAutoplay(ctx) ;
        }
        [Command("AutoPlayByGenre")]
        public async Task AutoPlayByGenre(CommandContext ctx, [RemainingText] string genre)
        {
            await MusicService.StartAutoPlayByGenre(ctx,genre) ;
        }
    }
}
