using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;


namespace Fafikv2.Services.OtherServices.Interfaces
{
    public interface ISongCollectionService
    {
        public Task AddToBase(LavalinkTrack track, CommandContext ctx);
        public Task<LavalinkTrack?> AutoPlay(LavalinkGuildConnection node, LavalinkTrack track);
        public Task<LavalinkTrack?> AutoPlayByGenre(LavalinkGuildConnection node, string? genre);
    }
}