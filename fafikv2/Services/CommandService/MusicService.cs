﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using Fafikv2.Data.Models;
using Fafikv2.Services.dbServices.Interfaces;
using Fafikv2.Services.OtherServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;


namespace Fafikv2.Services.CommandService
{
    public class MusicService
    {
        private readonly ISongCollectionService? _songCollectionService;
        private readonly IServerConfigService? _serverConfigService;
        private readonly IDatabaseContextQueueService? _databaseContextQueueService;
        private readonly Dictionary<ulong, SongServiceDictionary> _songServiceDictionaries = new();


        public MusicService(IServiceProvider servicesProvider)
        {
            _songCollectionService = servicesProvider.GetService<ISongCollectionService>();
            _serverConfigService = servicesProvider.GetService<IServerConfigService>();
            _databaseContextQueueService = servicesProvider.GetService<IDatabaseContextQueueService>();
        }

        

        public async Task JoinAsync(CommandContext ctx, DiscordChannel? channel = null)
        {
            if(!await IsConnected(ctx).ConfigureAwait(false)) return;


            var node = ctx
                .Client
                .GetLavalink()
                .ConnectedNodes
                .Values
                .First();
            if (node != null)
            {
                node.PlaybackFinished += Node_PlaybackFinished;
                node.GuildConnectionRemoved += Node_disconnected;


            }

            if (node != null) await node.ConnectAsync(channel).ConfigureAwait(false);
            await ctx.RespondAsync($"Joined {channel?.Name}!").ConfigureAwait(false);

            _songServiceDictionaries[ctx.Guild.Id] = new SongServiceDictionary { Queue = new List<LavalinkTrack>() ,Genre = string.Empty,AutoPlayOn = false};
        }

        private async Task Node_PlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs args)
        {
            var reason = args.Reason;

            if (reason == TrackEndReason.Finished)
            {
                await SkipNextInQueueAsync(sender, args).ConfigureAwait(false);
            }
            else
            {
                Console.WriteLine(args.Reason.ToString());
            }

        }

        private async Task SkipNextInQueueAsync(LavalinkGuildConnection node, TrackFinishEventArgs args)
        {
            var guildId = args.Player.Guild.Id;

            if ( _songServiceDictionaries.TryGetValue(guildId, out var dictionary) && dictionary.Queue is { Count: > 0 })
            {
                var finishedTrack = dictionary.Queue.First();
                dictionary.Queue.RemoveAt(0);

                var nextTrackMessage = "";
                if (dictionary.Queue.Count > 0)
                {
                    var nextTrack = dictionary.Queue.First();
                    nextTrackMessage = $"Next track: {nextTrack.Title}";
                    await node.PlayAsync(nextTrack).ConfigureAwait(false);
                }


                if (dictionary.AutoPlayOn && dictionary.Queue.Count == 0 && !dictionary.Genre.IsNullOrEmpty())
                {
                    if (_songCollectionService != null)
                    {
                        var autoNextTrack = await _songCollectionService.AutoPlayByGenre(node, dictionary.Genre).ConfigureAwait(false);
                        await node.PlayAsync(autoNextTrack).ConfigureAwait(false);
                        nextTrackMessage = $"Next track: {autoNextTrack.Title}";
                        dictionary.Queue.Add(autoNextTrack);
                        
                    }
                }
                else if (dictionary.AutoPlayOn && dictionary.Queue.Count==0)
                {
                    if (_songCollectionService != null)
                    {
                        var autoNextTrack = await _songCollectionService.AutoPlay(node, finishedTrack).ConfigureAwait(false);
                        await node.PlayAsync(autoNextTrack).ConfigureAwait(false);
                        nextTrackMessage = $"Next track: {autoNextTrack.Title}";
                        dictionary.Queue.Add(autoNextTrack);
                    }
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

            if (_songServiceDictionaries.TryGetValue(guild, out var dictionary))
            {
                dictionary.Queue?.Clear();

            }
            _songServiceDictionaries.Remove(guild, out _);
            return Task.CompletedTask;
        }

        public async Task LeaveAsync(CommandContext ctx)
        {
            if(!await IsConnected(ctx).ConfigureAwait(false)) return;

            var conn = ctx
                .Client
                .GetLavalink()
                .ConnectedNodes
                .Values
                .First()
                .GetGuildConnection(ctx.Member?.VoiceState.Guild);

            await conn.DisconnectAsync().ConfigureAwait(false);
            await ctx.RespondAsync($"Left {ctx.Guild.CurrentMember.VoiceState.Channel.Name}").ConfigureAwait(false);
        }

        public async Task PlayAsync(CommandContext ctx, string search)
        {
            if(!await IsConnected(ctx).ConfigureAwait(false)) return;

            var conn = ctx
                .Client
                .GetLavalink()
                .ConnectedNodes
                .Values
                .First()
                .GetGuildConnection(ctx.Member?.VoiceState.Guild);
            var node = ctx.Client.GetLavalink().ConnectedNodes.Values.First();



            LavalinkLoadResult loadResult;
            if (Uri.TryCreate(search, UriKind.Absolute, out var uri))
            {
                // If search is a valid URL, use the URI overload
                loadResult = await node.Rest.GetTracksAsync(uri).ConfigureAwait(false);
            }
            else
            {
                // Otherwise, treat it as a search query
                loadResult = await node.Rest.GetTracksAsync(search,LavalinkSearchType.SoundCloud).ConfigureAwait(false);
            }
            // node.Rest.GetTracksAsync()

            if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}.").ConfigureAwait(false);
                return;
            }

            LavalinkTrack track;
            try
            {
                track = loadResult.Tracks.First();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
           



            if (!_songServiceDictionaries.TryGetValue(ctx.Guild.Id, out var dictionary))
            {
                 
                 _songServiceDictionaries[ctx.Guild.Id] = new SongServiceDictionary() {Queue = new List<LavalinkTrack>()};
                 
                 

            }

            

            if (dictionary!.Queue!.Count == 0)
            {
                try
                {
                    await conn.PlayAsync(track).ConfigureAwait(false);
                    Console.WriteLine($"{track.Title}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.InnerException?.Message}");
                }
                dictionary.Queue.Add(track);
                await ctx.RespondAsync($"now playing {track.Title}!").ConfigureAwait(false);
            }
            else
            {

                var timeLeftAll = dictionary.Queue.Select(lavalinkTrack => lavalinkTrack.Length).ToList();

                TimeSpan timeLeft = new();


                timeLeft = timeLeftAll.Aggregate(timeLeft, (current, songTime) => current + songTime);
                Console.WriteLine($"{track.Title}");
                dictionary.Queue.Add(track);
                await ctx.RespondAsync($"Added {track.Title} to the queue!\n" + $"Song will be played in: {timeLeft.Minutes},{timeLeft.Seconds:D2}").ConfigureAwait(false);
            
            }

            if (_songCollectionService != null)
                await _songCollectionService.AddToBase(track, ctx).ConfigureAwait(false);
        }

        public async Task PauseAsync(CommandContext ctx)
        {
            if(!await IsConnected(ctx).ConfigureAwait(false)) return;

            var conn = ctx
                .Client
                .GetLavalink()
                .ConnectedNodes
                .Values
                .First()
                .GetGuildConnection(ctx.Member?.VoiceState.Guild);

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("there are no track loaded.").ConfigureAwait(false);
                return;
            }

            await conn.PauseAsync().ConfigureAwait(false);
            await ctx.RespondAsync("music is paused.").ConfigureAwait(false);


        }

        public async Task ResumeAsync(CommandContext ctx)
        {

            
            if (! await IsConnected(ctx).ConfigureAwait(false)) return;

            var conn = ctx
                .Client
                .GetLavalink()
                .ConnectedNodes
                .Values
                .First()
                .GetGuildConnection(ctx.Member?.VoiceState.Guild);


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


            if(!await IsConnected(ctx).ConfigureAwait(false)) return;

            var guildId = ctx.Channel.Guild.Id;
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member?.VoiceState.Guild);





            if (_songServiceDictionaries.TryGetValue(guildId, out var dictionary) && dictionary.Queue!.Count > 0)
            {
                var finishedTrack = dictionary.Queue.First();
                dictionary.Queue.RemoveAt(0);

                var nextTrackMessage = "";
                if (dictionary.Queue.Count > 0)
                {
                    var nextTrack = dictionary.Queue.First();
                    nextTrackMessage = $"Next track: {nextTrack.Title}";
                    await conn.PlayAsync(nextTrack).ConfigureAwait(false);
                }


                if (dictionary.AutoPlayOn && dictionary.Queue.Count == 0 && dictionary.Genre.IsNullOrEmpty())
                {
                    if (_songCollectionService != null)
                    {
                        var autoNextTrack = await _songCollectionService.AutoPlayByGenre(conn, dictionary.Genre).ConfigureAwait(false);
                        await conn.PlayAsync(autoNextTrack).ConfigureAwait(false);
                        nextTrackMessage = $"Next track: {autoNextTrack.Title}";
                        dictionary.Queue.Add(autoNextTrack);
                    }
                }
                else if (dictionary.AutoPlayOn && dictionary.Queue.Count == 0)
                {
                    if (_songCollectionService != null)
                    {
                        var autoNextTrack = await _songCollectionService.AutoPlay(conn, finishedTrack).ConfigureAwait(false);
                        await conn.PlayAsync(autoNextTrack).ConfigureAwait(false);
                        nextTrackMessage = $"Next track: {autoNextTrack.Title}";
                        dictionary.Queue.Add(autoNextTrack);
                    }
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



            if (_songServiceDictionaries.TryGetValue(guildId, out var dictionary) && dictionary.Queue?.Count > 0)
            {
                var songTitles = dictionary.Queue.Select(track => track.Title).ToList();
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

            await ctx.RespondAsync("Queue Is empty").ConfigureAwait(false);



        }
        
        public  async Task VolumeAsync(CommandContext ctx, int vol)
        {

            if(!await IsConnected(ctx).ConfigureAwait(false)) return;
            

            if (vol is < 0 or > 100)
            {
                await ctx.RespondAsync("volume must be between 0 and 100").ConfigureAwait(false);
                return;
            }

            await ctx
                .Client
                .GetLavalink()
                .ConnectedNodes
                .Values
                .First()
                .GetGuildConnection(ctx.Member?.VoiceState.Guild)
                .SetVolumeAsync(vol)
                .ConfigureAwait(false);

            await ctx.RespondAsync($"Volume changed to: {vol}").ConfigureAwait(false);
        }

        public async Task StartAutoplay(CommandContext ctx)
        {
            if (!await IsAutoplayEnabled(ctx).ConfigureAwait(false)) return;

            _songServiceDictionaries[ctx.Guild.Id].AutoPlayOn = true;
            _songServiceDictionaries[ctx.Guild.Id].Genre = string.Empty;
            
            
            await ctx.RespondAsync("Auto Play is on").ConfigureAwait(false);
        }

        public async Task StartAutoPlayByGenre(CommandContext ctx, string genre)
        {
            var result = await IsAutoplayEnabled(ctx).ConfigureAwait(false);
            if (!result) return; 

            _songServiceDictionaries[ctx.Guild.Id].Genre = genre;
            _songServiceDictionaries[ctx.Guild.Id].AutoPlayOn = true;

            await ctx.RespondAsync("Auto Play by genre is on").ConfigureAwait(false);
        }

        private async Task<bool> IsAutoplayEnabled(CommandContext ctx)
        {
            var result = await (_databaseContextQueueService?.EnqueueDatabaseTask(async () =>
                _serverConfigService != null && await _serverConfigService.IsAutoPlayEnable(Guid.Parse($"{ctx.Guild.Id:X32}")).ConfigureAwait(false))!).ConfigureAwait(false);

            if (!result)
            {
                await ctx.RespondAsync("Auto Play is disabled on this server.").ConfigureAwait(false);
                return false;
            }

            if (ctx.Member?.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.").ConfigureAwait(false);
                return false;
            }
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            if (conn != null) return true;
            await ctx.RespondAsync("Lavalink is not connected").ConfigureAwait(false);
            return false;

        }
        private async Task<bool> IsConnected(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();

            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established").ConfigureAwait(false);
                return false;
            }

            var node = lava.ConnectedNodes.Values.First();


            var voiceState = ctx.Member?.VoiceState;
            if (voiceState?.Channel == null)
            {
                Console.WriteLine($"{ctx.Member?.VoiceState?.Channel}");
                await ctx.RespondAsync("You need to be in a voice channel to use this command.").ConfigureAwait(false);
                return false;
            }
            var conn = node.GetGuildConnection(ctx.Member?.VoiceState.Guild);

            if (conn != null) return true;
            await ctx.RespondAsync("Lavalink is not connected.").ConfigureAwait(false);
            return false;

        }

    }

    
}
