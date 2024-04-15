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

namespace Fafikv2.Configuration.BotConfig
{
    public class BotClient
    {
        private static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands { get; set; }

        private readonly IUserService? _userService;
        private readonly IServerService? _serverService;
        private readonly IServerUsersService? _serverUsersService;
        private readonly IServerConfigService? _serverConfigService;
        private readonly IUserServerStatsService? _userServerStatsService;



        private readonly OnStartUpdateDatabaseQueue _onStartUpdateDatabaseQueue;

        public BotClient(IUserService? userService, 
            IServerService? serverService, 
            IServerUsersService? serverUsersService, 
            IServerConfigService? serverConfigService, 
            IUserServerStatsService? userServerStatsService)
        {
            _userService= userService;
            _serverService= serverService;
            _serverUsersService= serverUsersService;
            _serverConfigService= serverConfigService;
            _userServerStatsService= userServerStatsService;
            _onStartUpdateDatabaseQueue = new OnStartUpdateDatabaseQueue();
        }

       

        
        public async Task Initialize()
        {

            var jsonReader = new JsonReader();
            await jsonReader.ReadJson();


            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = jsonReader.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            Client = new DiscordClient(discordConfig);

            Client.Ready += Client_Ready;
            Client.MessageCreated += Client_MessageCreated;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.GuildMemberAdded += Client_GuildMemberAdded;
            Client.UnknownEvent += Client_UnknownEvent;
            

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { jsonReader.Prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<BaseCommands>();
            Commands.RegisterCommands<MusicCommands>();

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

            var lavalink = Client.UseLavalink();

            await Client.ConnectAsync();

            await lavalink.ConnectAsync(lavalinkConfig);


            Task.Run(async () =>
            {
                while (true)
                {
                    var task = await _onStartUpdateDatabaseQueue.DequeueAsync(CancellationToken.None);
                    if (task != null)
                    {
                        await task();
                    }
                    else
                    {
                        
                        await Task.Delay(1000);
                    }
                }
            });


            await Task.Delay(-1);
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
            var server =  args.Guild;



            await _onStartUpdateDatabaseQueue.Enqueue( async () => await UpdateDatabaseOnConnect(users, server));



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
                if (!user.IsBot)
                {
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

            
        }

        private static Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }

        private async Task Client_MessageCreated(DiscordClient sender, MessageCreateEventArgs args)
        {
            Console.WriteLine($"[{args.Message.CreationTimestamp}] {args.Message.Author.Username}: {args.Message.Content}");

            await _onStartUpdateDatabaseQueue.Enqueue(async () =>
            {
                if (args.Message.Content.StartsWith("!"))
                {
                    var userid = args.Author.Id;
                    var formatted = $"{userid:X32}";

                    var serverId = args.Guild.Id;
                    var sformatted = $"{serverId:X32}";

                    await _userService.UpdateUserBotInteractionsCount(Guid.Parse(formatted)).ConfigureAwait(false);
                    await _userServerStatsService.UpdateUserMessageServerCount(Guid.Parse(formatted), Guid.Parse(sformatted))
                        .ConfigureAwait(false);

                    await _userService.UpdateUserMessageCount(Guid.Parse(formatted)).ConfigureAwait(false);
                    await _userServerStatsService.UpdateUserBotInteractionsServerCount(Guid.Parse(formatted),
                        Guid.Parse(sformatted)).ConfigureAwait(false);
                }
                else
                {
                    var userid = args.Author.Id;
                    var formatted = $"{userid:X32}";

                    var serverId = args.Guild.Id;
                    var sformatted = $"{serverId:X32}";
                    await _userService.UpdateUserMessageCount(Guid.Parse(formatted)).ConfigureAwait(false);

                    await _userServerStatsService.UpdateUserMessageServerCount(Guid.Parse(formatted), Guid.Parse(sformatted))
                        .ConfigureAwait(false);
                }
            });
            
            
            
        }
    }
}
