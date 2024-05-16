using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Fafikv2.Services.CommandService;
using Fafikv2.Services.dbServices.Interfaces;
using Fafikv2.Services.dbServices;

namespace Fafikv2.Commands
{
    internal class MusicCommands : BaseCommandModule
    {
        private readonly MusicService _musicService=new ();
        private readonly IUserService _userService;

        private static IUserService _cachedUserService;

        /*public MusicCommands(MusicService musicService)
        {
            //_musicService = musicService;

            // Pobierz instancję IUserService tylko raz, aby uniknąć wielokrotnego tworzenia
            /*if (_cachedUserService == null)
            {
                _cachedUserService = new UserService(); // Tutaj należy utworzyć instancję UserService lub użyć metod do pobrania instancji zależnie od implementacji
            }

            _userService = _cachedUserService;
        }*/

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
            await MusicService.PauseAsync(ctx);
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
        



    }
}
