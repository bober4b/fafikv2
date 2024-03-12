using Microsoft.Extensions.DependencyInjection;
using Fafikv2.BotConfig;
using Microsoft.EntityFrameworkCore;
using Fafikv2.Data.DataContext;
using Fafikv2.Data.Models;

namespace Fafikv2
{

    public class Program
    {

        static async Task Main()
        {
            

            await new BotClient().Initialize();
        }


    }
}
