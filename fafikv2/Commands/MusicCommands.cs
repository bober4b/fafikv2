using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;


namespace fafikv2.Commands
{
    internal class MusicCommands : BaseCommandModule
    {
        [Command]
        public async Task Join(CommandContext ctx, DiscordChannel channel=null)
        {
           

            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink Connection is not established");
                return;
            }


            if (channel == null)
            {
                var voiceState = ctx.Member?.VoiceState;
                if (voiceState?.Channel == null)
                {
                    Console.WriteLine($"{ctx.Member.VoiceState?.Channel}");
                    await ctx.RespondAsync("You need to be in a voice channel to use this command.");
                    return;
                }

                channel = voiceState.Channel;

            }

            if (channel.Type != ChannelType.Voice)
            {
                
                await ctx.RespondAsync("Not a valid voice channel");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();
            await node.ConnectAsync(channel);
            await ctx.RespondAsync($"Joined {channel.Name}!");
        }

        [Command]
        public async Task Leave(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();

            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            var voiceState = ctx.Guild.CurrentMember?.VoiceState;
            var channel = voiceState?.Channel;

            if (channel == null)
            {
                await ctx.RespondAsync("I'm not in a voice channel.");
                return;
            }

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            var conn = node.GetGuildConnection(channel.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            await conn.DisconnectAsync();
            await ctx.RespondAsync($"Left {channel.Name}");
        }

        [Command]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected");
                return;
            }

            var loadResult = await node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed ||
                loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}.");
                return;
            }

            var track = loadResult.Tracks.First();

            await conn.PlayAsync(track);
            await ctx.RespondAsync($"now playing {track.Title}!");
            
        }


        
    }
}
