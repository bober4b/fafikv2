using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Fafikv2.Services.CommandService;

namespace Fafikv2.Commands
{
    public class MusicCommands : BaseCommandModule
    {
        private readonly MusicService _service;

        public MusicCommands(MusicService service)
        {
            _service = service;
        }


        [Command]
        public async Task Join(CommandContext ctx, DiscordChannel? channel = null)
        {

            await _service.JoinAsync(ctx, channel);

        }

        [Command]
        public async Task Leave(CommandContext ctx)
        {
            await _service.LeaveAsync(ctx);
        }

        [Command]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            await _service.PlayAsync(ctx, search);
        }

        [Command]
        public async Task Pause(CommandContext ctx)
        {
            await _service.PauseAsync(ctx);
        }

        [Command]
        public async Task Resume(CommandContext ctx)
        {
            await _service.ResumeAsync(ctx);
        }

        [Command]
        public async Task Skip(CommandContext ctx)
        {
            await _service.SkipAsync(ctx);
        }

        [Command]
        public async Task Queue(CommandContext ctx)
        {
            await _service.QueueAsync(ctx);
        }

        [Command]
        public async Task Volume(CommandContext ctx, int vol)
        {
            await _service.VolumeAsync(ctx, vol);
        }

        [Command("AutoPlay")]
        public async Task AutoPlay(CommandContext ctx)
        {
            await _service.StartAutoplay(ctx);
        }
        [Command("AutoPlayByGenre")]
        public async Task AutoPlayByGenre(CommandContext ctx, [RemainingText] string genre)
        {
            await _service.StartAutoPlayByGenre(ctx, genre);
        }
    }
}
