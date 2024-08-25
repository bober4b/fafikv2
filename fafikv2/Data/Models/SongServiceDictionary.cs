using DSharpPlus.Lavalink;

namespace Fafikv2.Data.Models
{
    public class SongServiceDictionary
    {
        public List<LavalinkTrack>? Queue { get; set; }
        public bool AutoPlayOn{ get; set; }
        public string? Genre { get; set; }
    }
}
