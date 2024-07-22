using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;

namespace Fafikv2.Services.OtherServices.Interfaces
{
    public interface ISongCollectionService
    {
        public Task AddToBase(LavalinkTrack  track, CommandContext ctx);
    }
}
