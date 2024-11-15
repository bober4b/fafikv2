using Fafikv2.Configuration.BotConfig;
using Fafikv2.Configuration.ConfigJSON;
using Fafikv2.Configuration.DependencyConfiguration;
using Fafikv2.Data.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.Extensions.Hosting;

namespace Fafikv2
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"C:\Users\bober\source\repos\bober4b\fafikv2\fafikv2\appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("Starting up the application...");

                // Budowanie hosta i uruchamianie go
                var host = CreateHostBuilder(args).Build();

                // Inicjalizacja bazy danych
                using (var scope = host.Services.CreateScope())
                {
                    var servicesProvider = scope.ServiceProvider;
                    await InitializeDatabase(servicesProvider);

                    // Tworzenie i inicjalizacja BotClient
                    var botClient = new BotClient(servicesProvider);
                    await botClient.Initialize();
                }

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }

        private static async Task InitializeDatabase(IServiceProvider servicesProvider)
        {
            using var scope = servicesProvider.CreateScope();
            var database = scope.ServiceProvider.GetRequiredService<DiscordBotDbContext>();
            await database.Database.MigrateAsync();
            await Task.Delay(1000);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog() // Konfiguracja Serilog jako globalny logger
                .ConfigureServices((context, services) =>
                {
                    // Wczytanie tokenu
                    var jsonReader = new JsonReader();
                    jsonReader.ReadJsonAsync().Wait();

                    // Konfiguracja serwisów
                    services
                        .AddDbContext<DiscordBotDbContext>()
                        .AddRepositories()
                        .AddServices()
                        .AddMusicService(jsonReader.Token);

                    // Dodanie BotClient do DI
                });
    }
}
