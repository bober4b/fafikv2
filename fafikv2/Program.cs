using Fafikv2.Configuration.BotConfig;
using Microsoft.Extensions.DependencyInjection;
using Fafikv2.Data.DataContext;
using Fafikv2.Configuration.DependencyConfiguration;
using Fafikv2.Services.dbSevices.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Fafikv2
{

    public class Program
    {

        static async Task Main()
        {

            var servicesProvider = new ServiceCollection()
                .AddDbContext<DiscordBotDbContext>()
                .AddRepositories()
                .AddServices()
                .BuildServiceProvider();

            await initializeDatabase(servicesProvider);

            await new BotClient(servicesProvider.GetRequiredService(typeof(IUserService)) as IUserService,
                    servicesProvider.GetRequiredService(typeof(IServerService)) as IServerService,
                    servicesProvider.GetRequiredService(typeof(IServerUsersService)) as IServerUsersService)
                .Initialize();
        }

        private static async Task initializeDatabase(ServiceProvider servicesProvider)
        {
            using (var scope = servicesProvider.CreateScope())
            {
                var database = scope.ServiceProvider.GetRequiredService<DiscordBotDbContext>();
                await database.Database.MigrateAsync();
                await Task.Delay(1000);
            }
        }
    }
}
