using Fafikv2.Services.dbServices;
using Microsoft.Extensions.DependencyInjection;
using Fafikv2.Services.dbServices.Interfaces;
using Fafikv2.Services.OtherServices;
using Fafikv2.Services.OtherServices.Interfaces;

namespace Fafikv2.Configuration.DependencyConfiguration
{
    public static class ServicesDependencyConfiguration
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IServerService, ServerService>();
            services.AddScoped<IServerUsersService, ServerUsersService>();
            services.AddScoped<IServerConfigService, ServerConfigService>();
            services.AddScoped<IUserServerStatsService, UserServerStatsService>();
            services.AddScoped<IBannedWordsService, BannedWordsService>();
            services.AddScoped<ISongsService, SongsService>();
            services.AddScoped<IUserPlayedSongsService, UserPlayedSongsService>();
            services.AddSingleton<IDatabaseContextQueueService, DatabaseContextQueueService>();
            services.AddSingleton<IAutoModerationService, AutoModerationService>();
            services.AddSingleton<ISongCollectionService, SongCollectionService>();
            services.AddSingleton<ISpotifyApiService, SpotifyApiService>();



            return services;
        }
    }
}
