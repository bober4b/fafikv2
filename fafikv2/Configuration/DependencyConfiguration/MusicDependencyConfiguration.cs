using Microsoft.Extensions.DependencyInjection;
using DSharpPlus;
using Fafikv2.Commands;
using Fafikv2.Services.CommandService;

namespace Fafikv2.Configuration.DependencyConfiguration
{
    public static class MusicDependencyConfiguration
    {
        public static IServiceCollection AddMusicService(this IServiceCollection services, string token)
        {

            services.AddSingleton(new DiscordClient(new DiscordConfiguration
                {
                    Intents = DiscordIntents.All,
                    Token = token,
                    TokenType = TokenType.Bot,
                    AutoReconnect = true
                }));

            

            services.AddSingleton<MusicPanel>();
            services.AddSingleton<MusicCommands>();
            services.AddSingleton<BaseCommands>();
            services.AddSingleton<AdminCommands>();
            services.AddSingleton<AdditionalMusicCommands>();
            services.AddSingleton<AdditionalMusicService>();
            services.AddSingleton<MusicService>();
            services.AddSingleton<AdminCommandService>();
            services.AddSingleton<BaseCommandService>();


            return services;
        }
    }
}