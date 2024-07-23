using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using Fafikv2.Services.OtherServices.Interfaces;

namespace Fafikv2.Services.CommandService
{
    public class MusicService
    {
        private readonly ISongCollectionService? _songCollectionService;


        public MusicService(ISongCollectionService? songCollectionService)
        {
            _songCollectionService= songCollectionService;
        }

        private readonly Dictionary<ulong, List<LavalinkTrack>> _queue = new();

        public async Task JoinAsync(CommandContext ctx, DiscordChannel? channel = null)
        {
            var lava = ctx.Client.GetLavalink();

            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink Connection is not established").ConfigureAwait(false);
                return;
            }


            if (channel == null)
            {
                var voiceState = ctx.Member?.VoiceState;
                if (voiceState?.Channel == null)
                {
                    if (ctx.Member != null) Console.WriteLine($"{ctx.Member.VoiceState?.Channel}");
                    await ctx.RespondAsync("You need to be in a voice channel to use this command.").ConfigureAwait(false);
                    return;
                }

                channel = voiceState.Channel;

            }

            if (channel.Type != ChannelType.Voice)
            {

                await ctx.RespondAsync("Not a valid voice channel").ConfigureAwait(false);
                return;
            }

            var node = lava.ConnectedNodes.Values.First();
            if (node != null)
            {
                node.PlaybackFinished += Node_PlaybackFinished;
                node.GuildConnectionRemoved += Node_disconnected;


            }

            if (node != null) await node.ConnectAsync(channel).ConfigureAwait(false);
            await ctx.RespondAsync($"Joined {channel.Name}!").ConfigureAwait(false);
        }

        private async Task Node_PlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs args)
        {
            var reason = args.Reason;

            if (reason == TrackEndReason.Finished)
            {
                await SkipNextInQueueAsync(sender, args).ConfigureAwait(false);
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
                    await node.PlayAsync(nextTrack).ConfigureAwait(false);
                }

                var finishedTrackMessage = $"Finished playing: {finishedTrack.Title}";
                var message = $"{finishedTrackMessage}\n{nextTrackMessage}";

                
                var textChannel = node.Guild.SystemChannel;
                await textChannel.SendMessageAsync(message).ConfigureAwait(false);
            }
        }

        private Task Node_disconnected(LavalinkGuildConnection sender, GuildConnectionRemovedEventArgs args)
        {
            var guild = sender.Guild.Id;

            if (_queue.TryGetValue(guild, out var queue))
            {
                queue.Clear();
            }

            return Task.CompletedTask;
        }

        public static async Task LeaveAsync(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();

            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established").ConfigureAwait(false);
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            var voiceState = ctx.Guild.CurrentMember?.VoiceState;
            var channel = voiceState?.Channel;

            if (channel == null)
            {
                await ctx.RespondAsync("I'm not in a voice channel.").ConfigureAwait(false);
                return;
            }

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.").ConfigureAwait(false);
                return;
            }

            var conn = node.GetGuildConnection(channel.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Not a valid voice channel.").ConfigureAwait(false);
                return;
            }

            await conn.DisconnectAsync().ConfigureAwait(false);
            await ctx.RespondAsync($"Left {channel.Name}").ConfigureAwait(false);
        }

        public async Task PlayAsync(CommandContext ctx, string search)
        {
            if (ctx.Member?.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.").ConfigureAwait(false);
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected").ConfigureAwait(false);
                return;
            }



            var loadResult = await node.Rest.GetTracksAsync(search,LavalinkSearchType.SoundCloud).ConfigureAwait(false);

            if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}.").ConfigureAwait(false);
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
                try
                {
                    await conn.PlayAsync(track).ConfigureAwait(false);
                    Console.WriteLine($"{conn.CurrentState.CurrentTrack.Title}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.InnerException?.Message}");
                }
                //await conn.PlayAsync(track);
                queue.Add(track);
                await ctx.RespondAsync($"now playing {track.Title}!").ConfigureAwait(false);
            }
            else
            {

                var timeLeftAll = queue.Select(lavalinkTrack => lavalinkTrack.Length).ToList();

                TimeSpan timeLeft = new();



                foreach (var songTime in timeLeftAll)
                {
                    timeLeft += songTime;
                }
                Console.WriteLine($"{conn.CurrentState.CurrentTrack.Title}");
                queue.Add(track);
                await ctx.RespondAsync($"Added {track.Title} to the queue!\n" + $"Song will be played in: {timeLeft.Minutes},{timeLeft.Seconds:D2}").ConfigureAwait(false);
            
            }

            await _songCollectionService.AddToBase(track, ctx).ConfigureAwait(false);
        }

        public static async Task PauseAsync(CommandContext ctx)
        {
            if (ctx.Member?.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.").ConfigureAwait(false);
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.").ConfigureAwait(false);
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("there are no track loaded.").ConfigureAwait(false);
                return;
            }

            await conn.PauseAsync().ConfigureAwait(false);
            await ctx.RespondAsync("music is paused.").ConfigureAwait(false);


        }

        public static async Task ResumeAsync(CommandContext ctx)
        {
            if (ctx.Member?.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.").ConfigureAwait(false);
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.").ConfigureAwait(false);
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("there are no track loaded.").ConfigureAwait(false);
                return;
            }

            await conn.ResumeAsync().ConfigureAwait(false);
            await ctx.RespondAsync("music is playing again.").ConfigureAwait(false);
        }

        public async Task SkipAsync(CommandContext ctx)
        {


            if (ctx.Member?.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.").ConfigureAwait(false);
                return;
            }

            var guildId = ctx.Channel.Guild.Id;
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.").ConfigureAwait(false);
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("there are no track loaded.").ConfigureAwait(false);
                return;
            }

            if (_queue.TryGetValue(guildId, out var queue) && queue.Count > 0)
            {
                var finishedTrack = queue.First();
                queue.RemoveAt(0);

                var nextTrackMessage = "";
                if (queue.Count > 0)
                {
                    var nextTrack = queue.First();
                    nextTrackMessage = $"Next track: {nextTrack.Title}";
                    await conn.PlayAsync(nextTrack).ConfigureAwait(false);
                }

                if (queue.Count == 0)
                {
                    await conn.StopAsync().ConfigureAwait(false);
                    var textChanneled = ctx.Guild.SystemChannel;
                    await textChanneled.SendMessageAsync("there is no more songs.").ConfigureAwait(false);
                    return;
                }

                var finishedTrackMessage = $"Finished playing: {finishedTrack.Title}";
                var message = $"{finishedTrackMessage}\n{nextTrackMessage}";

                
                var textChannel = ctx.Guild.SystemChannel;
                await textChannel.SendMessageAsync(message).ConfigureAwait(false);
            }
        }

        public async Task QueueAsync(CommandContext ctx)
        {


            var guildId = ctx.Channel.Guild.Id;



            if (_queue.TryGetValue(guildId, out var queue) && queue.Count > 0)
            {
                var songTitles = queue.Select(track => track.Title).ToList();
                var titles = "Songs Queue: \n";
                var i = 0;
                foreach (var song in songTitles)
                {
                    titles += $"{i}. {song}\n";
                    i++;
                }

                var textChannel = ctx.Guild.SystemChannel;
                await textChannel.SendMessageAsync(titles).ConfigureAwait(false);
            }




        }
        
        public static async Task VolumeAsync(CommandContext ctx, int vol)
        {


            var lava = ctx.Client.GetLavalink();

            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established").ConfigureAwait(false);
                return;
            }

            var node = lava.ConnectedNodes.Values.First();


            var voiceState = ctx.Member?.VoiceState;
            if (voiceState?.Channel == null)
            {
                Console.WriteLine($"{ctx.Member?.VoiceState?.Channel}");
                await ctx.RespondAsync("You need to be in a voice channel to use this command.").ConfigureAwait(false);
                return;
            }
            var conn = node.GetGuildConnection(ctx.Member?.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.").ConfigureAwait(false);
                return;
            }

            if (vol is < 0 or > 100)
            {
                await ctx.RespondAsync("volume must be between 0 and 100").ConfigureAwait(false);
                return;
            }

            await conn.SetVolumeAsync(vol).ConfigureAwait(false);
            await ctx.RespondAsync($"Volume changed to: {vol}").ConfigureAwait(false);
        }

    }
}
