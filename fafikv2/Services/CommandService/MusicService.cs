using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using Fafikv2.Commands.MessageCreator;
using Fafikv2.Data.DifferentClasses;
using Fafikv2.Data.Models;
using Fafikv2.Services.dbServices.Interfaces;
using Fafikv2.Services.OtherServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;


namespace Fafikv2.Services.CommandService
{
    public class MusicService
    {
        private readonly ISongCollectionService _songCollectionService;
        private readonly IServerConfigService _serverConfigService;
        private readonly IDatabaseContextQueueService _databaseContextQueueService;
        private readonly Dictionary<ulong, SongServiceDictionary> _songServiceDictionaries = new();



        public MusicService(IServiceProvider servicesProvider)
        {
            _songCollectionService = servicesProvider.GetRequiredService<ISongCollectionService>();
            _serverConfigService = servicesProvider.GetRequiredService<IServerConfigService>();
            _databaseContextQueueService = servicesProvider.GetRequiredService<IDatabaseContextQueueService>();
        }
        



        public async Task JoinAsync(CommandContext ctx, DiscordChannel? channel = null)
        {


            var lava = ctx.Client.GetLavalink();

            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync(MessagesComposition.EmbedMessageComposition("Connection","Bot is not connected to any channel"));
                return;
            }


            channel ??= ctx.Member?.VoiceState?.Channel;
            if (channel == null || channel.Type != ChannelType.Voice)
            {
                await SendEmbedMessage(ctx, "Invalid Channel", "You need to be in a valid voice channel to use this command.");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();
            node.PlaybackFinished += Node_PlaybackFinished;
            node.GuildConnectionRemoved += Node_disconnected;



            await node.ConnectAsync(channel);
            await SendEmbedMessage(ctx, "Joined", $"Joined {channel.Name}!");

            _songServiceDictionaries[ctx.Guild.Id] = new SongServiceDictionary
            {
                Queue = new List<LavalinkTrack>(),
                Genre = string.Empty,
                AutoPlayOn = false
            };
        }
        private async Task SendEmbedMessage(CommandContext ctx, string title, string description)
        {
            var embed = MessagesComposition.EmbedMessageComposition(title, description);
            await ctx.RespondAsync(embed);
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
            if (!_songServiceDictionaries.TryGetValue(args.Player.Guild.Id, out var dictionary) || dictionary.Queue!.Count == 0)
                return;

            var finishedTrack = dictionary.Queue.First();
            dictionary.Queue.RemoveAt(0);

            if (dictionary.Queue.Count > 0)
            {
                var nextTrack = dictionary.Queue.First();
                await PlayTrackAsync(node, nextTrack);
            }
            else if (dictionary.AutoPlayOn && !string.IsNullOrEmpty(dictionary.Genre))
            {
                var autoNextTrack = await _songCollectionService.AutoPlayByGenre(node, dictionary.Genre);
                if (autoNextTrack != null) dictionary.Queue.Add(autoNextTrack);
            }
            else if (dictionary.AutoPlayOn)
            {
                var autoNextTrack = await _songCollectionService.AutoPlay(node, finishedTrack);
                if (autoNextTrack != null) dictionary.Queue.Add(autoNextTrack);
            }

            
        }

        private async Task PlayTrackAsync(LavalinkGuildConnection node, LavalinkTrack track)
        {
            try
            {
                await node.PlayAsync(track);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to play track.");
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

        public  async Task LeaveAsync(CommandContext ctx)
        {


            await ExecuteIfConnected(ctx, async conn =>
            {
                try
                {
                    await conn.DisconnectAsync();
                }
                catch (Exception ex)
                {
                    await ctx.RespondAsync(MessagesComposition.EmbedMessageComposition("Error","An error occurred while leaving the room."));
                    Console.WriteLine(ex.Message);
                    return;
                }

                await conn.DisconnectAsync();
                await ctx.RespondAsync(MessagesComposition.EmbedMessageComposition("Left",$"Left {ctx.Guild.CurrentMember.VoiceState.Channel.Name}"));
            });
        }

        private  async Task PlayTrack(CommandContext ctx, LavalinkGuildConnection conn, LavalinkTrack track)
        {
            try
            {
                await conn.PlayAsync(track);

            }
            catch (Exception e)
            {
                await ctx.RespondAsync(MessagesComposition.EmbedMessageComposition("Error","An error occurred while playing the song. Please try again later."));
                Console.WriteLine($"{e.Message}");
            }
        }

        private  async Task PlayTrack(ComponentInteractionCreateEventArgs args, LavalinkGuildConnection conn, LavalinkTrack track)
        {
            try
            {
                await conn.PlayAsync(track);
            }
            catch (Exception e)
            {
                await HandleErrorAsync(args, e, "An error occurred while playing the song. Please try again later");

            }
        }

        public async Task PlayAsync(CommandContext ctx, string search)
        {

            await ExecuteIfConnected(ctx, async conn =>
            {
                var node = ctx.Client.GetLavalink().ConnectedNodes.Values.First();


                var loadResult = await TryLoadTrackAsync(node, search);
                if (loadResult == null) return;


                var track = loadResult.Tracks.First();
                if (!_songServiceDictionaries.TryGetValue(ctx.Guild.Id, out var dictionary))
                {
                    dictionary = new SongServiceDictionary { Queue = new List<LavalinkTrack>() };
                    _songServiceDictionaries[ctx.Guild.Id] = dictionary;
                }


                if (dictionary.Queue!.Count == 0)
                {
                    await PlayTrack(ctx, conn, track);
                    dictionary.Queue.Add(track);
                    await SendEmbedMessage(ctx, "Now Playing", $"Now playing {track.Title}!");
                }
                else
                {
                    dictionary.Queue.Add(track);
                    var timeLeft = TimeSpan.FromMilliseconds(dictionary.Queue.Sum(t => t.Length.TotalMilliseconds));
                    await SendEmbedMessage(ctx, "Added to Queue", $"Track {track.Title} added to the queue, playing in {timeLeft:mm\\:ss}.");
                }

                await AddTrackToCollectionAsync(ctx, track);
            });
        }

        private async Task<LavalinkLoadResult?> TryLoadTrackAsync(LavalinkNodeConnection node, string search)
        {
            try
            {
                return Uri.TryCreate(search, UriKind.Absolute, out var uri)
                    ? await node.Rest.GetTracksAsync(uri)
                    : await node.Rest.GetTracksAsync(search, LavalinkSearchType.SoundCloud);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading track for search: {Search}", search);
                return null;
            }
        }

        private async Task AddTrackToCollectionAsync(CommandContext ctx, LavalinkTrack track)
        {
            try
            {
                await _songCollectionService.AddToBase(track, ctx);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding track to collection.");
            }
        }

        public async Task SkipAsync(CommandContext ctx)
        {
            await ExecuteIfConnected(ctx, async conn =>
            {
                if (!_songServiceDictionaries.TryGetValue(ctx.Channel.Guild.Id, out var dictionary) ||
                    dictionary.Queue == null ||
                    dictionary.Queue.Count == 0)
                {
                    await SendEmbedMessage(ctx, "Error", "No tracks in the queue to skip.");
                    return;
                }


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
                    else if (dictionary.Queue.Count==0)
                    {
                        await conn.StopAsync();
                    }

                    var finishedTrackMessage = $"Finished playing: {finishedTrack.Title}";
                    var nextTrackMessage = dictionary.Queue.Count > 0 ? $"Next track: {dictionary.Queue.First().Title}" : "No more tracks in queue.";
                    var message = $"{finishedTrackMessage}\n{nextTrackMessage}";

                    await SendEmbedMessage(ctx, "Track Finished", message);

            });
        }

        private async Task HandleErrorAsync(CommandContext ctx, Exception e, string customMessage)
        {
            await SendEmbedMessage(ctx, "Error", customMessage);
            Log.Error(e.Message);
        }

        private async Task HandleErrorAsync(ComponentInteractionCreateEventArgs args, Exception e, string customMessage)
        {
            await SendMessage(args, customMessage,"Error");
            Log.Error(e.Message);

        }

        private async Task<LavalinkTrack?> GetAutoNextTrack(LavalinkGuildConnection conn, SongServiceDictionary dictionary, LavalinkTrack finishedTrack)
        {
            if (!string.IsNullOrEmpty(dictionary.Genre))
            {
                return await _songCollectionService.AutoPlayByGenre(conn, dictionary.Genre);
            }

            return await _songCollectionService.AutoPlay(conn, finishedTrack);
        }

        public async Task PauseAsync(CommandContext ctx)
        {

            await ExecuteIfConnected(ctx, async conn =>
            {

                if (conn.CurrentState.CurrentTrack == null)
                {
                    await SendEmbedMessage(ctx,"Pause", "there are no track loaded.");
                    return;
                }

                try
                {
                    await conn.PauseAsync();
                }
                catch (Exception e)
                {

                    await HandleErrorAsync(ctx, e, "An error occurred while pausing the song. Please try again later.");
                    return;
                }

                await SendEmbedMessage(ctx,"Paused", "music is paused.");

            });
        }

        public async Task ResumeAsync(CommandContext ctx)
        {

            await ExecuteIfConnected(ctx, async conn =>
            {
                if (conn.CurrentState.CurrentTrack == null)
                {
                    await SendEmbedMessage(ctx, "Resume", "there are no track loaded.");
                    return;
                }

                try
                {
                    await conn.ResumeAsync();
                }
                catch (Exception e)
                {
                    await HandleErrorAsync(ctx, e, "An error occurred while resuming the song. Please try again later.");
                    return;
                }

                await SendEmbedMessage(ctx, "Resumed", "music is playing again.");
            });
        }

        public async Task QueueAsync(CommandContext ctx)
        {
            if (!_songServiceDictionaries.TryGetValue(ctx.Guild.Id, out var dictionary) ||
                dictionary.Queue == null ||
                dictionary.Queue.Count == 0)
            {
                await ctx.RespondAsync(MessagesComposition.EmbedQueueComposition("Queue is empty"));
                return;
            }

            var songTitles = dictionary.Queue
                .Select((track, index) => $"{index + 1}. {track.Title}")
                .ToList();

            var message = string.Join("\n", songTitles);
            await ctx.RespondAsync(MessagesComposition.EmbedQueueComposition(message));
        }

        public async Task VolumeAsync(CommandContext ctx, int vol)
        {

            if (vol is < 0 or > 100)
            {
                await SendEmbedMessage(ctx, "Volume", "volume must be between 0 and 100");
                return;
            }

            await ExecuteIfConnected(ctx, async conn =>
            {

                try
                {
                    await conn.SetVolumeAsync(vol);
                    _songServiceDictionaries[ctx.Guild.Id].Volume = vol;
                    await SendEmbedMessage(ctx, "Volume", $"Volume changed to: {vol}");
                }
                catch (Exception e)
                {
                    await HandleErrorAsync(ctx, e, "An error occurred while setting the volume. Please try again later.");
                }
            });
        }

        public async Task StartAutoplay(CommandContext ctx)
        {
            if (!await IsAutoplayEnabled(ctx)) return;

            _songServiceDictionaries[ctx.Guild.Id].AutoPlayOn = true;
            _songServiceDictionaries[ctx.Guild.Id].Genre = string.Empty;


            await SendEmbedMessage(ctx, "Auto Play", "Auto Play is on");
        }

        public async Task StartAutoPlayByGenre(CommandContext ctx, string genre)
        {
            var result = await IsAutoplayEnabled(ctx);
            if (!result) return;

            _songServiceDictionaries[ctx.Guild.Id].Genre = genre;
            _songServiceDictionaries[ctx.Guild.Id].AutoPlayOn = true;

            await SendEmbedMessage(ctx, "Auto Play", "Auto Play by genre is on");
        }

        private async Task<bool> IsAutoplayEnabled(CommandContext ctx)
        {
            var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
                await _serverConfigService.IsAutoPlayEnable(ctx.Guild.Id.ToGuid()));

            if (result) return await IsConnected(ctx) != null;
            await ctx.RespondAsync(MessagesComposition.EmbedMessageComposition("Auto Play", "Auto Play is disabled on this server."));
            return false;

        }

        private async Task<bool> IsAutoplayEnabled(DiscordClient client, ComponentInteractionCreateEventArgs args)
        {

            var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
                await _serverConfigService.IsAutoPlayEnable(args.Guild.Id.ToGuid()));

            if (result)
            {
                var connection = await IsConnectedPanel(client, args); 
                return connection != null;
            }

            await SendMessage(args, "Auto Play is disabled on this server.","Auto Play");
            return false;
        }

        private async Task<LavalinkGuildConnection?> IsConnected(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();

            if (!lava.ConnectedNodes.Any())
            {
                await SendEmbedMessage(ctx, "Info", "The Lavalink connection is not established");
                return null;
            }

            var node = lava.ConnectedNodes.Values.First();


            var voiceState = ctx.Member?.VoiceState;
            if (voiceState?.Channel == null)
            {
                await SendEmbedMessage(ctx, "Info", "You need to be in a voice channel to use this command.");
                return null;
            }
            var conn = node.GetGuildConnection(ctx.Member?.VoiceState.Guild);

            if (conn != null) return conn;
            await SendEmbedMessage(ctx, "Info", "Lavalink is not connected.");
            return null;

        }

        private static async Task<LavalinkGuildConnection?> IsConnectedPanel(DiscordClient client, ComponentInteractionCreateEventArgs args)
        {

            var guildId = args.Guild.Id;
            var memberId = args.User.Id;
            var guild = await client.GetGuildAsync(guildId);
            var member = await guild.GetMemberAsync(memberId);



            var lava = client.GetLavalink();

            if (!lava.ConnectedNodes.Any())
            { 
                await SendMessage(args,"The Lavalink connection is not established","Not connected");
                return null;
            }

            var node = lava.ConnectedNodes.Values.First();


            var voiceState = member.VoiceState;
            if (voiceState?.Channel == null)
            {
                await SendMessage(args, "You need to be in a voice channel to use this command.", "Not connected");
                return null;
            }
            var conn = node.GetGuildConnection(guild);

            if (conn != null) return conn;
            await SendMessage(args, "Lavalink is not connected.", "Not connected");
            return null;
        }

        private static async Task SendMessage(ComponentInteractionCreateEventArgs args, string context,string title)
        {
            var message=MessagesComposition.EmbedMessageComposition(title,context);
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(message)
                    .AsEphemeral());
        }

        private async Task ExecuteIfConnected(CommandContext ctx, Func<LavalinkGuildConnection, Task> action)
        {
            var conn = await IsConnected(ctx);
            if (conn == null) return;

            await action(conn);
        }

        public  Task<SongServiceDictionary> GetMusicDictionary(DiscordGuild guild)
        {
            if (!_songServiceDictionaries.TryGetValue(guild.Id, out _))
            {
                _songServiceDictionaries[guild.Id] = new SongServiceDictionary { Queue = new List<LavalinkTrack>() };
            }
            
            return Task.FromResult(_songServiceDictionaries[guild.Id]);
        }

        public async Task<bool> SkipFromPanel(DiscordClient client, ComponentInteractionCreateEventArgs args)
        {
            var guildId = args.Guild.Id;
            var guild = await client.GetGuildAsync(guildId);


            var lava = await IsConnectedPanel(client, args);

            if (lava == null) 
                return true;


            if (_songServiceDictionaries.TryGetValue(guild.Id, out var dictionary) && dictionary.Queue?.Count > 0)
            {
                var finishedTrack = dictionary.Queue.First();
                dictionary.Queue.RemoveAt(0);

                if (dictionary.Queue.Count > 0)
                {
                    await PlayTrack(args, lava, dictionary.Queue[0]);
                }
                else if (dictionary.AutoPlayOn && dictionary.Queue.Count == 0)
                {
                    var autoNextTrack = await GetAutoNextTrack(lava, dictionary, finishedTrack);
                    if (autoNextTrack != null)
                    {
                        await PlayTrack(args, lava, autoNextTrack); 
                        dictionary.Queue.Add(autoNextTrack);
                    }
                }
                else if (dictionary.Queue.Count == 0)
                {
                    await lava.StopAsync();
                }

                var finishedTrackMessage = $"Finished playing: {finishedTrack.Title}";
                var nextTrackMessage = dictionary.Queue.Count > 0 ? $"Next track: {dictionary.Queue.First().Title}" : "No more tracks in queue.";
                var message = $"{finishedTrackMessage}\n{nextTrackMessage}";

                await SendMessage(args, message,"Song");
            }

            return true;
        }

        public async Task<bool> ResumeAsyncFromPanel(DiscordClient client,
            ComponentInteractionCreateEventArgs args)
        {


            var lava = await IsConnectedPanel(client, args);

            if (lava == null)
                return true;

            if (lava.CurrentState.CurrentTrack == null)
            {
                await SendMessage(args,"there are no track loaded.", "Not connected");
                return true;
            }

            try
            {
                await lava.ResumeAsync();
            }
            catch (Exception e)
            {
                await HandleErrorAsync(args, e, "An error occurred while resuming the song. Please try again later.");
                return true;
            }

            await SendMessage(args, "music is playing again.","Resumed");



            return true;


        }

        public async Task<bool> AutoPlayFromPanel(DiscordClient client, ComponentInteractionCreateEventArgs args)
        {
            var guildId = args.Guild.Id;

            if (!await IsAutoplayEnabled(client,args)) return true;

            _songServiceDictionaries[guildId].AutoPlayOn = true;
            _songServiceDictionaries[guildId].Genre = string.Empty;


            await SendMessage(args, "Auto Play is on.", "Auto Play");
            return true;
        }

        public async Task<bool> PauseFromPanel(DiscordClient client, ComponentInteractionCreateEventArgs args)
        {


            var lava = await IsConnectedPanel(client, args);

            if (lava == null)
                return true;


            if (lava.CurrentState.CurrentTrack == null)
            {
                await SendMessage(args, "there are no track loaded.", "Pause");
                return true;
            }

            try
            {
                await lava.PauseAsync();
            }
            catch (Exception e)
            {
                await HandleErrorAsync(args, e, "An error occurred while pausing the song. Please try again later.");
                return true;
            }

            await SendMessage(args, "music is paused.", "Pause");

            return true;

        }

        public Task<string> QueueFromPanel( ComponentInteractionCreateEventArgs args)
        {
            var guildId = args.Guild.Id;

            if (!_songServiceDictionaries.TryGetValue(guildId, out var dictionary) || dictionary.Queue == null || dictionary.Queue.Count == 0)
            {
                return Task.FromResult("Queue is empty");
            }

            var songTitles = dictionary.Queue.Select((track, index) => $"{index + 1}. {track.Title}").ToList();
            return Task.FromResult(string.Join("\n", songTitles));
        }

    }
}