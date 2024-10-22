using Fafikv2.Repositories;
using Fafikv2.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Fafikv2.Configuration.DependencyConfiguration
{
    public static class RepositoryDependencyConfiguration
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IServerRepository, ServerRepository>();
            services.AddScoped<IServerUsersRepository, ServerUsersRepository>();
            services.AddScoped<IServerConfigRepository, ServerConfigRepository>();
            services.AddScoped<IUserServerStatsRepository, UserServerStatsRepository>();
            services.AddScoped<IBannedWordsRepository, BannedWordsRepository>();
            services.AddScoped<ISongsRepository, SongsRepository>();
            services.AddScoped<IUserPlayedSongsRepository, UserPlayedSongsRepository>();

            return services;
        }
    }
}
