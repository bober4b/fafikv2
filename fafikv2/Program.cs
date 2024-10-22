using Fafikv2.Configuration.BotConfig;
using Fafikv2.Configuration.ConfigJSON;
using Fafikv2.Configuration.DependencyConfiguration;
using Fafikv2.Data.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fafikv2
{
    public class Program
    {
        private static async Task Main()
        {
            var jsonReader = new JsonReader();
            await jsonReader.ReadJson();

            var servicesProvider = new ServiceCollection()
                .AddDbContext<DiscordBotDbContext>()
                .AddRepositories()
                .AddServices()
                .AddMusicService(jsonReader.Token)
                .BuildServiceProvider();
                


            await InitializeDatabase(servicesProvider);

            await new BotClient(servicesProvider)
                    .Initialize();
        }
        private static async Task InitializeDatabase(IServiceProvider servicesProvider)
        {
            using var scope = servicesProvider.CreateScope();
            var database = scope.ServiceProvider.GetRequiredService<DiscordBotDbContext>();
            await database.Database.MigrateAsync();
            await Task.Delay(1000);
        }
    }
}
