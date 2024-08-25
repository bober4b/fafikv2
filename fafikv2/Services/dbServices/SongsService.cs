using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;
using Fafikv2.Services.dbServices.Interfaces;

namespace Fafikv2.Services.dbServices
{
    public class SongsService : ISongsService
    {
        private readonly ISongsRepository _songsRepository;

        public SongsService(ISongsRepository songsRepository)
        {
            _songsRepository= songsRepository;
        }
        public async Task<bool> Add(Song song)
        {
            var added = await _songsRepository.HasBeenAdded(song.Title, song.Artist).ConfigureAwait(false);
            if (added)
            {
                Console.WriteLine("Song already exists in the database.");
                return false;
            }

            await _songsRepository.AddSong(song).ConfigureAwait(false);
            return true;
        }

        public async Task<Song?> Get(string? title, string? artist)
        {
            var result = await _songsRepository.Get(title, artist).ConfigureAwait(false);
            return result ?? null;
        }

        public async Task<IEnumerable<Song>> GetSongByGenre(string? genre)
        {
            var result = await _songsRepository.GetSongByGenre(genre).ConfigureAwait(false);
            return result ;
        }

        public async Task<IEnumerable<Song>> GetSongsByGenreAndUser(string? genre, Guid userId)
        {
            var result = await _songsRepository.GetSongsByGenreAndUser(genre, userId).ConfigureAwait(false);
            return result ;
        }

        public async Task<IEnumerable<Song>> GetSongsByUser(Guid userId)
        {
            var result = await _songsRepository.GetSongsByUser(userId).ConfigureAwait(false);
            return result ;
        }
    }
}
