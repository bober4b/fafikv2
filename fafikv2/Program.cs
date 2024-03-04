using DSharpPlus;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Fafikv2.Config;
using Fafikv2.Commands;
using DSharpPlus.EventArgs;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using Fafikv2.BotConfig;


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