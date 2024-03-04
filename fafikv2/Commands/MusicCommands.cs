﻿using System;
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
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using Fafikv2.Services;

namespace Fafikv2.Commands
{
    internal class MusicCommands : BaseCommandModule
    {
        private readonly Dictionary<ulong, List<LavalinkTrack>> _queue = new();
        private MusicService _musicService=new();
        

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
        



    }
}
