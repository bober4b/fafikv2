using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

namespace Fafikv2.Services
{
    public class MusicService
    {
        
        private readonly Dictionary<ulong, List<LavalinkTrack>> _queue = new();


        
        

        public async Task JoinAsync(CommandContext ctx, DiscordChannel channel = null)
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
            if (node != null)
            {
                node.PlaybackFinished += Node_PlaybackFinished;
                node.GuildConnectionRemoved += Node_disconnected;

            }

            await node.ConnectAsync(channel);
            await ctx.RespondAsync($"Joined {channel.Name}!");
        }


        private async Task Node_PlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs args)
        {
            TrackEndReason reason = args.Reason;

            if (reason == TrackEndReason.Finished)
            {
                await SkipNextInQueueAsync(sender, args);
            }
            
        }

        private async Task SkipNextInQueueAsync(LavalinkGuildConnection node, TrackFinishEventArgs args)
        {
            var guildId = args.Player.Guild.Id;

            if (_queue.TryGetValue(guildId, out var queue) && queue.Count > 0)
            {
                var finishedTrack = queue.First();
                queue.RemoveAt(0);

                var nextTrackMessage = "";
                if (queue.Count > 0)
                {
                    var nextTrack = queue.First();
                    nextTrackMessage = $"Next track: {nextTrack.Title}";
                    await node.PlayAsync(nextTrack);
                }

                var finishedTrackMessage = $"Finished playing: {finishedTrack.Title}";
                var message = $"{finishedTrackMessage}\n{nextTrackMessage}";

                // Wysyłanie wiadomości do kanału tekstu (można zmienić na inną metodę wysyłania wiadomości)
                var textChannel = node.Guild.SystemChannel;
                await textChannel.SendMessageAsync(message);
            }
        }

        private async Task Node_disconnected(LavalinkGuildConnection sender, GuildConnectionRemovedEventArgs args)
        {
            var guildid = sender.Guild.Id;

            if(_queue.TryGetValue(guildid, out var queue))
            {
                queue.Clear();
            }

        }




        public async Task LeaveAsync(CommandContext ctx)
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

        public async Task PlayAsync(CommandContext ctx, string search)
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



            if (!_queue.TryGetValue(ctx.Guild.Id, out var queue))
            {
                queue = new List<LavalinkTrack>();
                _queue[ctx.Guild.Id] = queue;
            }


            if (queue.Count == 0)
            {
                await conn.PlayAsync(track);
                queue.Add(track);
                await ctx.RespondAsync($"now playing {track.Title}!");
            }
            else
            {
                queue.Add(track);
                await ctx.RespondAsync($"Added {track.Title} to the queue!");
            }
        }




    }
}
