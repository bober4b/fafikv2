﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Fafikv2.Commands;
using Fafikv2.Configuration.ConfigJSON;
using Fafikv2.Data.Models;
using Fafikv2.Services.dbServices.Interfaces;
using Fafikv2.Services.OtherServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;


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
        private readonly ServiceProvider _serviceProvider;





        public BotClient(ServiceProvider servicesProvider)
        {


            _userService = servicesProvider.GetRequiredService(typeof(IUserService)) as IUserService ?? throw new InvalidOperationException();
            _serverService = servicesProvider.GetRequiredService(typeof(IServerService)) as IServerService ?? throw new InvalidOperationException();
            _serverUsersService = servicesProvider.GetRequiredService(typeof(IServerUsersService)) as IServerUsersService ?? throw new InvalidOperationException();
            _serverConfigService = servicesProvider.GetRequiredService(typeof(IServerConfigService)) as IServerConfigService ?? throw new InvalidOperationException();
            _userServerStatsService = servicesProvider.GetRequiredService(typeof(IUserServerStatsService)) as IUserServerStatsService ?? throw new InvalidOperationException();
            _databaseContextQueueService = servicesProvider.GetRequiredService(typeof(IDatabaseContextQueueService)) as IDatabaseContextQueueService ?? throw new InvalidOperationException();
            _autoModerationService = servicesProvider.GetService<IAutoModerationService>() ?? throw new InvalidOperationException();
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

        private async Task Client_VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs args)
        {
            var botVoiceState = args.Guild.CurrentMember?.VoiceState;
            if (botVoiceState == null || botVoiceState.Channel == null )
            {
                return;
            }
            var botChannel = botVoiceState.Channel;
            var usersInChannel = botChannel.Users.Count(user => !user.IsBot);

            if (usersInChannel == 0)
            {
                var lavalink = _client.GetLavalink();
                var nodeConnection = lavalink.GetGuildConnection(botChannel.Guild);
                if (nodeConnection != null)
                {
                    await nodeConnection.DisconnectAsync();
                    var textChannel = args.Guild.SystemChannel;
                    await textChannel.SendMessageAsync("Bot left the channel due to inactivity.");
                }
            }
        }

        private static Task Client_UnknownEvent(DiscordClient sender, UnknownEventArgs args)
        {
            Console.WriteLine("xD");
            return Task.CompletedTask;
        }

        private static Task Client_GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs args)
        {
            Console.WriteLine($"{args.Member.Username}");
            return Task.CompletedTask;
        }

        private async Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs args)
        {
            var users = await args.Guild.GetAllMembersAsync();
            var server = args.Guild;



            await _databaseContextQueueService.EnqueueDatabaseTask(async () => await UpdateDatabaseOnConnect(users, server));



        }

        public async Task UpdateDatabaseOnConnect(IReadOnlyCollection<DiscordMember> users, DiscordGuild server)
        {
            var serverGuidToFormat = server.Id;
            var toFormatted = $"{serverGuidToFormat:X32}";
            var sConfig = new ServerConfig
            {
                Id = Guid.NewGuid(),
                ServerId = Guid.Parse(toFormatted)
            };

            var server1 = new Server
            {
                Name = server.Name,
                Id = Guid.Parse(toFormatted),
                ConfigId = sConfig.Id
            };

            await _serverService.AddServer(server1);
            await _serverConfigService.AddServerConfig(sConfig);
            foreach (var user in users)
            {
                if (user.IsBot) continue;
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
                Console.WriteLine($"dodano: {user.Username} {user.Id} {server.Name}");



                var serverUser = new ServerUsers
                {
                    ServerId = server1.Id,
                    UserId = useradd.Id,
                    UserServerStatsId = statsId,
                    Id = Guid.NewGuid(),


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


        }

        private static Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }

        private async Task Client_MessageCreated(DiscordClient sender, MessageCreateEventArgs args)
        {
            Console.WriteLine($"[{args.Message.CreationTimestamp}] {args.Message.Author.Username}: {args.Message.Content}");
            if (args.Channel.IsPrivate) return;
            if (args.Author.IsBot) return;
            var result = await _autoModerationService.AutoModerator(args);
            if (!result) return;

            await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
                {
                    if (args.Message.Content.StartsWith("!"))
                    {
                        var userid = args.Author.Id;
                        var formatted = $"{userid:X32}";

                        var serverId = args.Guild.Id;
                        var sformatted = $"{serverId:X32}";

                        await _userService.UpdateUserBotInteractionsCount(Guid.Parse(formatted));
                        await _userServerStatsService
                            .UpdateUserMessageServerCount(Guid.Parse(formatted), Guid.Parse(sformatted))
                             ;

                        await _userService.UpdateUserMessageCount(Guid.Parse(formatted));
                        await _userServerStatsService.UpdateUserBotInteractionsServerCount(Guid.Parse(formatted),
                            Guid.Parse(sformatted));

                    }
                    else
                    {
                        var userid = args.Author.Id;
                        var formatted = $"{userid:X32}";

                        var serverId = args.Guild.Id;
                        var sformatted = $"{serverId:X32}";
                        await _userService.UpdateUserMessageCount(Guid.Parse(formatted));

                        await _userServerStatsService
                            .UpdateUserMessageServerCount(Guid.Parse(formatted), Guid.Parse(sformatted))
                             ;
                    }

                });




        }
    }
}
