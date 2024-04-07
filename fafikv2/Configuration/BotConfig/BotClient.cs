using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Fafikv2.Commands;
using Fafikv2.Configuration.ConfigJSON;
using Fafikv2.Data.Models;
using Fafikv2.Services.dbSevices.Interfaces;

namespace Fafikv2.Configuration.BotConfig
{
    public class BotClient
    {
        private static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands { get; set; }

        private readonly IUserService _userService;
        private readonly IServerService _serverService;
        private readonly IServerUsersService _serverUsersService;



        private readonly OnStartUpdateDatabaseQueue _onStartUpdateDatabaseQueue;

        public BotClient(IUserService userService, IServerService serverService, IServerUsersService serverUsersService)
        {
            _userService= userService;
            _serverService= serverService;
            _serverUsersService= serverUsersService;
            _onStartUpdateDatabaseQueue = new OnStartUpdateDatabaseQueue();
        }

       

        public async Task Initialize()
        {

            var jsonReader = new JSONReader();
            await jsonReader.ReadJSON();


            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = jsonReader.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            Client = new DiscordClient(discordConfig);

            Client.Ready += Client_Ready;
            Client.MessageCreated += Client_MessageCreated;
            Client.GuildAvailable += Client_GuildAvailable;

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { jsonReader.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<Base_Commands>();
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
            var server1 = new Server
            {
                Name = server.Name,
                Id = Guid.Parse(toFormatted)
            };

            await _serverService.AddServer(server1);
            foreach (var user in users)
            {
                if (!user.IsBot)
                {
                    var value = user.Id;
                    var formatted = $"{value:X32}";
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
                    Console.WriteLine($"dodano: {user.Username}");

                    var serverUser = new ServerUsers
                    {
                        ServerId = server1.Id,
                        UserId = useradd.Id
                    };
                    await _serverUsersService.AddServerUsers(serverUser);
                }
            }

            
        }

        private static Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }

        private static Task Client_MessageCreated(DiscordClient sender, MessageCreateEventArgs args)
        {
            Console.WriteLine($"[{args.Message.CreationTimestamp}] {args.Message.Author.Username}: {args.Message.Content}");
            return Task.CompletedTask;
        }
    }
}
