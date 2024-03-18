using Microsoft.Extensions.DependencyInjection;
using Fafikv2.BotConfig;
using Microsoft.EntityFrameworkCore;
using Fafikv2.Data.DataContext;
using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;
using Fafikv2.Repositories;
using Fafikv2.Configuration.DependencyConfiguration;





namespace Fafikv2
{

    public class Program
    {

        static async Task Main()
        {

            var servicesProvider = new ServiceCollection()
                .AddDbContext<DiscordBotDbContext>()
                .AddRepositories()
                .BuildServiceProvider();
                

                

                await new BotClient().Initialize();
        }


    }
}
