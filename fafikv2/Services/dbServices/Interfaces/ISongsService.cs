using Fafikv2.Data.Models;

namespace Fafikv2.Services.dbServices.Interfaces
{
    public interface ISongsService
    {
        public Task<bool> Add(Song song);
        public Task<Song?> Get(string? title, string? artist);
        public Task<IEnumerable<Song>> GetSongByGenre(string? genre);
        public Task<IEnumerable<Song>> GetSongsByGenreAndUser(string? genre, Guid userId);
        public Task<IEnumerable<Song>> GetSongsByUser(Guid userId);
        public Task<Song?> GetRandomSong();

    }
}
