using DSharpPlus;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using Fafikv2.Commands;
using Fafikv2.Configuration.BotConfig;
using Fafikv2.Configuration.ConfigJSON;
using Fafikv2.Data.Models;
using Fafikv2.Services.dbSevices.Interfaces;
using Fafikv2.Services.dbSevices;

namespace Fafikv2.BotConfig
{
    public class BotClient
    {
        private static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands { get; set; }

        private readonly IUserService _userService;
        private readonly OnStartUpdateDatabaseQueue _onStartUpdateDatabaseQueue;

        public BotClient(IUserService userService)
        {
            _userService= userService;
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




            await _onStartUpdateDatabaseQueue.Enqueue( async () => await UpdateUsersOnConnect(users));



        }

        public async Task UpdateUsersOnConnect(IReadOnlyCollection<DiscordMember> users)
        {
            //Console.WriteLine("uopdate halo");
            foreach (var user in users)
            {
                if (!user.IsBot)
                {
                    ulong value = user.Id;
                    string formatedguid = $"{value:X32}";
                    var useradd = new User
                    {
                        Name = user.Username,
                        DisplayName = user.DisplayName,
                        Id = Guid.Parse(formatedguid)

                    };
                    await _userService.AddUser(useradd);
                    Console.WriteLine($"dodano: {user.Username}");
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
