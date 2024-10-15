using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Fafikv2.Services.CommandService;

namespace Fafikv2.Commands
{
    public class MusicCommands : BaseCommandModule
    {
        public static MusicService Service { get; set; } = null!;


        [Command]
        public async Task Join(CommandContext ctx, DiscordChannel? channel=null)
        {

            await Service.JoinAsync(ctx, channel) ;

        }

        [Command]
        public async Task Leave(CommandContext ctx)
        {
            await MusicService.LeaveAsync(ctx) ;
        }

        [Command]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            await Service.PlayAsync(ctx, search) ;
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
            await Service.SkipAsync(ctx) ;
        }

        [Command]
        public async Task Queue(CommandContext ctx)
        {
            await Service.QueueAsync(ctx);
        }

        [Command]
        public async Task Volume(CommandContext ctx, int vol)
        {
            await MusicService.VolumeAsync(ctx, vol);
        }

        [Command("AutoPlay")]
        public async Task AutoPlay(CommandContext ctx)
        {
            await Service.StartAutoplay(ctx) ;
        }
        [Command("AutoPlayByGenre")]
        public async Task AutoPlayByGenre(CommandContext ctx, [RemainingText] string genre)
        {
            await Service.StartAutoPlayByGenre(ctx,genre) ;
        }
    }
}
