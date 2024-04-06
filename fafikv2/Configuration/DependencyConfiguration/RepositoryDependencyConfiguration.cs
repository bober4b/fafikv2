using Fafikv2.Repositories;
using Fafikv2.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Fafikv2.Configuration.DependencyConfiguration
{
    public static class RepositoryDependencyConfiguration
    {
        public static IServiceCollection AddRepositories(this IServiceCollection Services)
        {
            Services.AddScoped<IUserRepository, UserRepository>();
            Services.AddScoped<IServerRepository, ServerRepository>();
            Services.AddScoped<IServerUsersRepository, ServerUsersRepository>();

            return Services;
        }
    }
}
