using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

namespace Fafikv2.Services.OtherServices.Interfaces
{
    public interface ISongCollectionService
    {
        public Task AddToBase(LavalinkTrack  track, CommandContext ctx);
        public Task<LavalinkTrack> AutoPlay(LavalinkGuildConnection node, LavalinkTrack track);
        public Task<LavalinkTrack> AutoPlayByGenre(LavalinkGuildConnection node, string genre);
    }
}