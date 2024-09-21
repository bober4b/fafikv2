using Fafikv2.Data.Models;
namespace Fafikv2.Repositories.Interfaces
{
    public interface ISongsRepository
    {
        public Task AddSong(Song  song);
        public Task RemoveSong(Song song);
        public Task<Song?> Get(string? title, string? artist);
        public Task<bool> HasBeenAdded(string? title, string? artist);
        public Task<IEnumerable<Song>> GetSongByGenre(string? genre);
        public Task<IEnumerable<Song?>> GetSongsByGenreAndUser(string? genre, Guid userId);
        public Task<IEnumerable<Song?>> GetSongsByUser(Guid userId);
        public Task<IEnumerable<Song>> GetAll();
    }
}
