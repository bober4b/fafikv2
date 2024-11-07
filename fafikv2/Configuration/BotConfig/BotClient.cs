using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Fafikv2.Commands;
using Fafikv2.Commands.MessageCreator;
using Fafikv2.Configuration.ConfigJSON;
using Fafikv2.Data.DifferentClasses;
using Fafikv2.Data.Models;
using Fafikv2.Services.dbServices.Interfaces;
using Fafikv2.Services.OtherServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;



namespace Fafikv2.Configuration.BotConfig
{
    public class BotClient
    {
        private readonly DiscordClient _client;
        private static CommandsNextExtension? Commands { get; set; }

        private readonly IUserService _userService;
        private readonly IServerService _serverService;
        private readonly IServerUsersService _serverUsersService;
        private readonly IServerConfigService _serverConfigService;
        private readonly IUserServerStatsService _userServerStatsService;
        private readonly IDatabaseContextQueueService _databaseContextQueueService;
        private readonly IAutoModerationService _autoModerationService;
        private readonly IServiceProvider _serviceProvider;


        public BotClient(IServiceProvider servicesProvider)
        {
            _userService = servicesProvider.GetRequiredService<IUserService>();
            _serverService = servicesProvider.GetRequiredService<IServerService>();
            _serverUsersService = servicesProvider.GetRequiredService<IServerUsersService>();
            _serverConfigService = servicesProvider.GetRequiredService<IServerConfigService>();
            _userServerStatsService = servicesProvider.GetRequiredService<IUserServerStatsService>();
            _databaseContextQueueService = servicesProvider.GetRequiredService<IDatabaseContextQueueService>();
            _autoModerationService = servicesProvider.GetRequiredService<IAutoModerationService>();
            _serviceProvider = servicesProvider;

            _client = _serviceProvider.GetRequiredService<DiscordClient>();

        }


        public async Task Initialize()
        {

            var jsonReader = new JsonReader();
            await jsonReader.ReadJson();


            _client.Ready += Client_Ready;
            _client.MessageCreated += Client_MessageCreated;
            _client.GuildAvailable += Client_GuildAvailable;
            _client.GuildMemberAdded += Client_GuildMemberAdded;
            _client.UnknownEvent += Client_UnknownEvent;
            _client.VoiceStateUpdated += Client_VoiceStateUpdated;
            _client.GuildMemberUpdated += Client_GuildMemberUpdated;
                


            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { jsonReader.Prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
                Services = _serviceProvider
            };

            Commands = _client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<BaseCommands>();
            Commands.RegisterCommands<MusicCommands>();
            Commands.RegisterCommands<AdminCommands>();
            Commands.RegisterCommands<AdditionalMusicCommands>();
            Commands.RegisterCommands<MusicPanel>();



            var endpoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1",
                Port = 2333
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "youshallnotpass",
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            var lavalink = _client.UseLavalink();

            await _client.ConnectAsync();

            await lavalink.ConnectAsync(lavalinkConfig);


            _ = Task.Run(async () =>
            {
                while (true)
                {
                    var task = await _databaseContextQueueService.DequeueDatabaseTask(CancellationToken.None);
                    if (task != null)
                    {

                        await task();
                        _databaseContextQueueService.DisplayQueueCount();
                    }
                    else
                    {

                        await Task.Delay(1000);
                    }
                }
                // ReSharper disable once FunctionNeverReturns
            });


            await Task.Delay(-1);
        }

        private async Task Client_GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs args)
        {
            if(args.Member.IsBot)return;

            var oldGlobalName = args.UsernameBefore; 
            var newGlobalName = args.UsernameAfter;
            var oldNickname = args.NicknameBefore;
            var newNickname = args.NicknameAfter;

            if (oldGlobalName != newGlobalName)
            {

                Log.Information($"User's global username changed: {oldGlobalName} -> {newGlobalName}");
                await _databaseContextQueueService.EnqueueDatabaseTask(async () => 
                    await _userService.UpdateUser(args.Member.Id.ToGuid(), newGlobalName));
            }

            if (oldNickname != newNickname)
            {
                var oldDisplayName = oldNickname ?? oldGlobalName;  
                var newDisplayName = newNickname ?? newGlobalName;


                Log.Information($"User's nickname on server {args.Guild.Name} changed: {oldDisplayName} -> {newDisplayName}");

                await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
                    await _userServerStatsService.UpdateUserServerStats(args.Member.Id.ToGuid(), args.Guild.Id.ToGuid(),
                        newDisplayName));

                //Log. Information ($"Pseudonim użytkownika na serwerze {args.Guild.Name} zmieniony: {oldDisplayName} -> {newDisplayName}");
            }
        }

        private async Task Client_VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs args)
        {
            var botVoiceState = args.Guild.CurrentMember?.VoiceState;
            if (botVoiceState == null || botVoiceState.Channel == null )
            {
                return;
            }
            var botChannel = botVoiceState.Channel;
            if (botChannel.Users.All(user => user.IsBot))
            {
                var lavalink = _client.GetLavalink();
                var nodeConnection = lavalink.GetGuildConnection(botChannel.Guild);
                if (nodeConnection != null)
                {
                    await nodeConnection.DisconnectAsync();
                    var textChannel = args.Guild.SystemChannel;
                    await textChannel
                        .SendMessageAsync(MessagesComposition
                            .EmbedMessageComposition( "Activity","Bot left the channel due to inactivity."));
                }
            }
        }

        private static Task Client_UnknownEvent(DiscordClient sender, UnknownEventArgs args)
        {
            Log.Debug("Unknown event received.");
            return Task.CompletedTask;
        }

        private async Task Client_GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs args)
        {
            Log.Information($"User {args.Member.Username} joined.");

            if(args.Member.IsBot)return;
            var server = (await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
                await _serverService.GetServer(args.Guild.Id.ToGuid())));
            await AddUser(args.Member, server);
        }

        private async Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs args)
        {
            var users = await args.Guild.GetAllMembersAsync();
            var server = args.Guild;

            await _databaseContextQueueService.EnqueueDatabaseTask(async () => await UpdateDatabaseOnConnect(users, server));

        }

        public async Task UpdateDatabaseOnConnect(IReadOnlyCollection<DiscordMember> users, DiscordGuild server)
        {
            var serverGuid = server.Id.ToGuid();
            var sConfig = new ServerConfig { Id = Guid.NewGuid(), ServerId = serverGuid };
            var serverEntity = new Server { Name = server.Name, Id = serverGuid, ConfigId = sConfig.Id };

            await _serverService.AddServer(serverEntity);
            await _serverConfigService.AddServerConfig(sConfig);

            var addUserTasks = users
                .Where(user => !user.IsBot)
                .Select(user => AddUser(user, serverEntity))
                .ToArray();

            await Task.WhenAll(addUserTasks);
        }


        private async Task AddUser(DiscordMember user, Server? server)
        {
            if(user.IsBot )return;
            var value = user.Id;
            var formatted = $"{value:X32}";
            var statsId = Guid.NewGuid();
            var useradd = new User
            {
                Name = user.Username,
                Id = Guid.Parse(formatted),
                BotInteractionGlobal = 0,
                GlobalKarma = 0,
                MessagesCountGlobal = 0,
                UserLevel = 0

            };
            await _userService.AddUser(useradd);
            Log.Information($"User added: {user.Username}, ID: {user.Id}, Server: {server!.Name}");




            var serverUser = new ServerUsers
            {
                ServerId = server.Id,
                UserId = useradd.Id,
                UserServerStatsId = statsId,
                Id = Guid.NewGuid()
            };


            var userStats = new UserServerStats
            {
                Id = statsId,
                ServerKarma = 0,
                MessagesCountServer = 0,
                BotInteractionServer = 0,
                DisplayName = user.DisplayName,
                ServerUsers = serverUser,
                ServerUserId = serverUser.Id
            };

            await _serverUsersService.AddServerUsers(serverUser);
            await _userServerStatsService.AddUserServerStats(userStats);
        }

        private static Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
        {
            Log.Information("Bot is ready.");
            return Task.CompletedTask;
        }

        private async Task Client_MessageCreated(DiscordClient sender, MessageCreateEventArgs args)
        {
            Log.Debug($"Message received: [{args.Message.CreationTimestamp}] {args.Message.Author.Username}: {args.Message.Content}");
            if (args.Channel.IsPrivate || args.Author.IsBot) return;

            if (await _autoModerationService.AutoModerator(args))
            {
                await _databaseContextQueueService.EnqueueDatabaseTask(() => UpdateUserStats(args));
            }
        }

        private async Task UpdateUserStats(MessageCreateEventArgs args)
        {
            var userGuid = args.Author.Id.ToGuid();
            var serverGuid = args.Guild.Id.ToGuid();

            await _userService.UpdateUserMessageCount(userGuid);
            await _userServerStatsService.UpdateUserMessageServerCount(userGuid, serverGuid);

            if (args.Message.Content.StartsWith("!"))
            {
                await _userService.UpdateUserBotInteractionsCount(userGuid);
                await _userServerStatsService.UpdateUserBotInteractionsServerCount(userGuid, serverGuid);
            }
        }
    }
}