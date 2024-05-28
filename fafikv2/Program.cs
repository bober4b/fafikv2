using Fafikv2.Configuration.BotConfig;
using Microsoft.Extensions.DependencyInjection;
using Fafikv2.Data.DataContext;
using Fafikv2.Configuration.DependencyConfiguration;
using Microsoft.EntityFrameworkCore;

namespace Fafikv2
{
    public class Program
    {
        private static async Task Main()
        {

            var servicesProvider = new ServiceCollection()
                .AddDbContext<DiscordBotDbContext>()
                .AddRepositories()
                .AddServices()
                .BuildServiceProvider();

            await InitializeDatabase(servicesProvider).ConfigureAwait(false);

            await new BotClient(servicesProvider)
                    .Initialize().ConfigureAwait(false);
        }
        private static async Task InitializeDatabase(IServiceProvider servicesProvider)
        {
            using var scope = servicesProvider.CreateScope();
            var database = scope.ServiceProvider.GetRequiredService<DiscordBotDbContext>();
            await database.Database.MigrateAsync().ConfigureAwait(false);
            await Task.Delay(1000).ConfigureAwait(false);
        }
    }
}
