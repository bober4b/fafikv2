using Fafikv2.Data.DifferentClasses;
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
            _songsRepository = songsRepository;
        }
        public async Task<bool> Add(Song song, Task<string?[]> genres)
        {
            var added = await _songsRepository.HasBeenAdded(song.Title, song.Artist);
            if (added)
            {
                Console.WriteLine("Song already exists in the database.");
                return false;
            }

            song.Genres = string.Join(", ",  await genres);

            await _songsRepository.AddSong(song);
            return true;
        }

        public async Task<Song?> Get(string? title, string? artist)
        {
            var result = await _songsRepository.Get(title, artist);
            return result ?? null;
        }

        public async Task<IEnumerable<Song>> GetSongByGenre(string? genre)
        {
            var result = await _songsRepository.GetSongByGenre(genre);
            return result;
        }

        public async Task<IEnumerable<Song>> GetSongsByGenreAndUser(string? genre, Guid userId)
        {
            IEnumerable<Song> result = (await _songsRepository.GetSongsByGenreAndUser(genre, userId))!;
            return result;
        }

        public async Task<IEnumerable<Song>> GetSongsByUser(Guid userId)
        {
            var result = await _songsRepository.GetSongsByUser(userId);
            return result!;
        }

        public async Task<Song?> GetRandomSong()
        {
            var result = await _songsRepository.GetAll();


            return result!.Randomize(1).FirstOrDefault();
        }
    }
}
