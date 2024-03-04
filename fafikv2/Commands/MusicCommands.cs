using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink.EventArgs;
using Fafikv2.Services;

namespace Fafikv2.Commands
{
    internal class MusicCommands : BaseCommandModule
    {
        private readonly MusicService _musicService=new();
        

        [Command]
        public async Task Join(CommandContext ctx, DiscordChannel channel=null)
        {

            await _musicService.JoinAsync(ctx, channel);

        }

        [Command]
        public async Task Leave(CommandContext ctx)
        {
            await _musicService.LeaveAsync(ctx);
        }

        [Command]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            await _musicService.PlayAsync(ctx, search);
        }

        [Command]
        public async Task Pause(CommandContext ctx)
        {
            await _musicService.PauseAsync(ctx);
        }

        [Command]
        public async Task Resume(CommandContext ctx)
        {
            await _musicService.ResumeAsync(ctx);
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
            await _musicService.VolumeAsync(ctx, vol);
        }
        



    }
}
