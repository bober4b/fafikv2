using DSharpPlus;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Fafikv2.Config;
using Fafikv2.Commands;
using DSharpPlus.EventArgs;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using fafikv2.Commands;

namespace Fafikv2
{

    public class Program
    {
        private static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands { get; set; }


        static async Task Main(string[] args)
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

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { jsonReader.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<Commandstest>();
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
            

            await Task.Delay(-1);
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