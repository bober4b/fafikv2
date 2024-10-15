using DSharpPlus;
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
                    if (ctx.Member != null) Console.WriteLine($"{ctx.Member.VoiceState?.Channel}");
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

            if (node != null) await node.ConnectAsync(channel);
            await ctx.RespondAsync($"Joined {channel.Name}!");

            _songServiceDictionaries[ctx.Guild.Id] = new SongServiceDictionary { Queue = new List<LavalinkTrack>(), Genre = string.Empty, AutoPlayOn = false };

        }

        private async Task Node_PlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs args)
        {
            var reason = args.Reason;

            if (reason == TrackEndReason.Finished)
            {
                await SkipNextInQueueAsync(sender, args);
            }
            else
            {
                Console.WriteLine(args.Reason.ToString());
            }

        }

        private async Task SkipNextInQueueAsync(LavalinkGuildConnection node, TrackFinishEventArgs args)
        {
            var guildId = args.Player.Guild.Id;

            if (_songServiceDictionaries.TryGetValue(guildId, out var dictionary) && dictionary.Queue is { Count: > 0 })
            {
                var finishedTrack = dictionary.Queue.First();
                dictionary.Queue.RemoveAt(0);

                var nextTrackMessage = "";
                if (dictionary.Queue.Count > 0)
                {
                    var nextTrack = dictionary.Queue.First();
                    nextTrackMessage = $"Next track: {nextTrack.Title}";
                    try
                    {
                        await node.PlayAsync(nextTrack);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return;
                    }
                    
                }


                if (dictionary.AutoPlayOn && dictionary.Queue.Count == 0 && !dictionary.Genre.IsNullOrEmpty())
                {
                    if (_songCollectionService != null)
                    {
                        var autoNextTrack = await _songCollectionService.AutoPlayByGenre(node, dictionary.Genre);
                        await node.PlayAsync(autoNextTrack);
                        nextTrackMessage = $"Next track: {autoNextTrack?.Title}";
                        if (autoNextTrack != null) dictionary.Queue.Add(autoNextTrack);
                    }
                }
                else if (dictionary.AutoPlayOn && dictionary.Queue.Count == 0)
                {
                    if (_songCollectionService != null )
                    {
                        var autoNextTrack = await _songCollectionService.AutoPlay(node, finishedTrack);
                        await node.PlayAsync(autoNextTrack);
                        if (autoNextTrack != null)
                        {
                            nextTrackMessage = $"Next track: {autoNextTrack.Title}";
                            dictionary.Queue.Add(autoNextTrack);
                        }
                    }
                }

                var finishedTrackMessage = $"Finished playing: {finishedTrack.Title}";
                var message = $"{finishedTrackMessage}\n{nextTrackMessage}";


                var textChannel = node.Guild.SystemChannel;
                await textChannel.SendMessageAsync(message);
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

        public static async Task LeaveAsync(CommandContext ctx)
        {


            await ExecuteIfConnected(ctx, async conn =>
            {
                try
                {
                    await conn.DisconnectAsync();
                }
                catch (Exception ex)
                {
                    await ctx.RespondAsync("Wystąpił błąd podczas opuszczania pokoju.");
                    Console.WriteLine(ex.Message);
                    return;
                }

                await conn.DisconnectAsync();
                await ctx.RespondAsync($"Left {ctx.Guild.CurrentMember.VoiceState.Channel.Name}");
            });
        }

        private async Task PlayTrack(CommandContext ctx, LavalinkGuildConnection conn, LavalinkTrack track)
        {
            try
            {
                await conn.PlayAsync(track);

                //await ctx.RespondAsync($"Now playing {track.Title}!");
            }
            catch (Exception e)
            {
                await ctx.RespondAsync("Wystąpił błąd podczas odtwarzania piosenki. Spróbuj ponownie później.");
                Console.WriteLine($"{e.Message}");
            }
        }

        private static async Task<LavalinkLoadResult> LoadTrack(LavalinkNodeConnection node, string search)
        {
            try
            {
                if (Uri.TryCreate(search, UriKind.Absolute, out var uri))
                {
                    return await node.Rest.GetTracksAsync(uri);
                }

                return await node.Rest.GetTracksAsync(search, LavalinkSearchType.SoundCloud);
            }
            catch (Exception ex)
            {
                throw new Exception("Wystąpił błąd podczas pobierania piosenki.", ex);
            }
        }

        public async Task PlayAsync(CommandContext ctx, string search)
        {

            await ExecuteIfConnected(ctx, async conn =>
            {
                var node = ctx.Client.GetLavalink().ConnectedNodes.Values.First();


                var loadResult = await LoadTrack(node, search);

                // node.Rest.GetTracksAsync()

                if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
                {
                    await ctx.RespondAsync($"Track search failed for {search}.");
                    return;
                }

                LavalinkTrack track;
                try
                {
                    track = loadResult.Tracks.First();
                }
                catch (Exception ex)
                {
                    await ctx.RespondAsync("Wystąpił błąd podczas pobierania piosenki. Spróbuj ponownie później.");
                    Console.WriteLine(ex.Message);
                    return;
                }


                if (!_songServiceDictionaries.TryGetValue(ctx.Guild.Id, out var dictionary))
                {
                    _songServiceDictionaries[ctx.Guild.Id] = new SongServiceDictionary { Queue = new List<LavalinkTrack>() };
                }


                if (dictionary is { Queue.Count: 0 })
                {
                    await PlayTrack(ctx, conn, track);
                    dictionary.Queue.Add(track);


                    await ctx.RespondAsync($"now playing {track.Title}!");
                }
                else
                {
                    if (dictionary?.Queue != null)
                    {
                        var timeLeft = TimeSpan.FromMilliseconds(dictionary.Queue.Sum(timeSpan => timeSpan.Length.TotalMilliseconds));
                        Console.WriteLine($"{track.Title}");
                        dictionary.Queue.Add(track);


                   

                        await ctx.RespondAsync(
                            $"Utwór {track.Title} zostanie odtworzony za: {timeLeft.Minutes}:{timeLeft.Seconds:D2}.");
                    }
                }

                if (_songCollectionService != null)
                    try
                    {
                        await _songCollectionService.AddToBase(track, ctx);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
            });
        }

        public async Task SkipAsync(CommandContext ctx)
        {
            await ExecuteIfConnected(ctx, async conn =>
            {
                if (_songServiceDictionaries.TryGetValue(ctx.Channel.Guild.Id, out var dictionary) && dictionary.Queue is
                    {
                        Count: > 0
                    })
                {
                    var finishedTrack = dictionary.Queue.First();
                    dictionary.Queue.RemoveAt(0);

                    if (dictionary.Queue.Count > 0)
                    {
                        await PlayTrack(ctx, conn, dictionary.Queue[0]);
                    }
                    else if (dictionary.AutoPlayOn && dictionary.Queue.Count == 0)
                    {
                        var autoNextTrack = await GetAutoNextTrack(conn, dictionary, finishedTrack);
                        if (autoNextTrack != null)
                        {
                            await PlayTrack(ctx, conn, autoNextTrack);
                            dictionary.Queue.Add(autoNextTrack);
                        }
                    }

                    var finishedTrackMessage = $"Finished playing: {finishedTrack.Title}";
                    var nextTrackMessage = dictionary.Queue.Count > 0 ? $"Next track: {dictionary.Queue.First().Title}" : "No more tracks in queue.";
                    var message = $"{finishedTrackMessage}\n{nextTrackMessage}";

                    var textChannel = ctx.Guild.SystemChannel;
                    await textChannel.SendMessageAsync(message);
                }
            });
        }

        private async Task<LavalinkTrack?> GetAutoNextTrack(LavalinkGuildConnection conn, SongServiceDictionary dictionary, LavalinkTrack finishedTrack)
        {
            if (!string.IsNullOrEmpty(dictionary.Genre))
            {
                return await _songCollectionService!.AutoPlayByGenre(conn, dictionary.Genre);
            }

            return await _songCollectionService!.AutoPlay(conn, finishedTrack);
        }

        public static async Task PauseAsync(CommandContext ctx)
        {

            await ExecuteIfConnected(ctx, async conn =>
            {

                if (conn.CurrentState.CurrentTrack == null)
                {
                    await ctx.RespondAsync("there are no track loaded.");
                    return;
                }

                try
                {
                    await conn.PauseAsync();
                }
                catch (Exception e)
                {
                    await ctx.RespondAsync("Wystąpił błąd podczas pauzowania piosenki. Spróbuj ponownie później.")
                        ;
                    Console.WriteLine($"{e.Message}");
                    return;
                }

                await ctx.RespondAsync("music is paused.");

            });
        }

        public static async Task ResumeAsync(CommandContext ctx)
        {



            await ExecuteIfConnected(ctx, async conn =>
            {
                if (conn.CurrentState.CurrentTrack == null)
                {
                    await ctx.RespondAsync("there are no track loaded.");
                    return;
                }

                try
                {
                    await conn.ResumeAsync();
                }
                catch (Exception e)
                {
                    await ctx.RespondAsync("Wystąpił błąd podczas wznawiania piosenki. Spróbuj ponownie później.")
                        ;
                    Console.WriteLine($"{e.Message}");
                    return;
                }

                await ctx.RespondAsync("music is playing again.");
            });
        }

        public async Task QueueAsync(CommandContext ctx)
        {


            if (!_songServiceDictionaries.TryGetValue(ctx.Guild.Id, out var dictionary) || dictionary.Queue == null || dictionary.Queue.Count == 0)
            {
                await ctx.RespondAsync("Queue is empty");
                return;
            }

            var songTitles = dictionary.Queue.Select((track, index) => $"{index + 1}. {track.Title}").ToList();
            var titles = string.Join("\n", songTitles);

            var textChannel = ctx.Guild.SystemChannel;
            await textChannel.SendMessageAsync($"Songs Queue:\n{titles}");



        }

        public static async Task VolumeAsync(CommandContext ctx, int vol)
        {

            if (vol is < 0 or > 100)
            {
                await ctx.RespondAsync("volume must be between 0 and 100");
                return;
            }

            await ExecuteIfConnected(ctx, async conn =>
            {

                try
                {
                    await conn.SetVolumeAsync(vol);
                }
                catch (Exception e)
                {
                    await ctx.RespondAsync("Wystąpił błąd podczas ustawiania głośności. Spróbuj ponownie później.")
                        ;
                    Console.WriteLine($"{e.Message}");
                    return;
                }

                await ctx.RespondAsync($"Volume changed to: {vol}");
            });
        }

        public async Task StartAutoplay(CommandContext ctx)
        {
            if (!await IsAutoplayEnabled(ctx)) return;

            _songServiceDictionaries[ctx.Guild.Id].AutoPlayOn = true;
            _songServiceDictionaries[ctx.Guild.Id].Genre = string.Empty;


            await ctx.RespondAsync("Auto Play is on");
        }

        public async Task StartAutoPlayByGenre(CommandContext ctx, string genre)
        {
            var result = await IsAutoplayEnabled(ctx);
            if (!result) return;

            _songServiceDictionaries[ctx.Guild.Id].Genre = genre;
            _songServiceDictionaries[ctx.Guild.Id].AutoPlayOn = true;

            await ctx.RespondAsync("Auto Play by genre is on");
        }

        private async Task<bool> IsAutoplayEnabled(CommandContext ctx)
        {
            var result = await (_databaseContextQueueService?.EnqueueDatabaseTask(async () =>
                _serverConfigService != null && await _serverConfigService.IsAutoPlayEnable(Guid.Parse($"{ctx.Guild.Id:X32}")))!);

            if (!result)
            {
                await ctx.RespondAsync("Auto Play is disabled on this server.");
                return false;
            }

            if (await IsConnected(ctx) == null) return false;

            return true;

        }
        private static async Task<LavalinkGuildConnection?> IsConnected(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();

            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return null;
            }

            var node = lava.ConnectedNodes.Values.First();


            var voiceState = ctx.Member?.VoiceState;
            if (voiceState?.Channel == null)
            {
                Console.WriteLine($"{ctx.Member?.VoiceState?.Channel}");
                await ctx.RespondAsync("You need to be in a voice channel to use this command.");
                return null;
            }
            var conn = node.GetGuildConnection(ctx.Member?.VoiceState.Guild);

            if (conn != null) return conn;
            await ctx.RespondAsync("Lavalink is not connected.");
            return null;

        }

        private static async Task ExecuteIfConnected(CommandContext ctx, Func<LavalinkGuildConnection, Task> action)
        {
            var conn = await IsConnected(ctx);
            if (conn == null) return;

            await action(conn);
        }

    }
}