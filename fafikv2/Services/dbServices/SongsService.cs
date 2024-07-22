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
        public async Task Add(Song song)
        {
            var added = await _songsRepository.HasBeenAdded(song.Name, song.Artist).ConfigureAwait(false);
            if (added) return;
            await _songsRepository.AddSong(song).ConfigureAwait(false);
        }

        public async Task<Song?> Get(string name, string artist)
        {
            var result = await _songsRepository.Get(name, artist).ConfigureAwait(false);
            return result ?? null;
        }

        public async Task<IEnumerable<Song>> GetSongByGenre(string genre)
        {
            var result = await _songsRepository.GetSongByGenre(genre).ConfigureAwait(false);
            return result ;
        }

        public async Task<IEnumerable<Song>> GetSongsByGenreAndUser(string genre, Guid userId)
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
