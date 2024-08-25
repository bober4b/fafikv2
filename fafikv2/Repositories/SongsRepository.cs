using Fafikv2.Data.DataContext;
using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fafikv2.Repositories
{
    public class SongsRepository : ISongsRepository
    {
        private readonly DiscordBotDbContext _context;
        public SongsRepository(DiscordBotDbContext context)
        {
            _context=context;
        }

        public async Task AddSong(Song song)
        {
            _context.Songs.Add(song);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }


        public async Task RemoveSong(Song song)
        {
            throw new NotImplementedException();
        }

        public async Task<Song?> Get(string? title, string? artist)
        {
            var result = await _context.Songs
                .FirstOrDefaultAsync(x => x.Artist == artist && x.Title == title)
                .ConfigureAwait(false);
            return result;
        }

        public async Task<bool> HasBeenAdded(string? title, string? artist)
        {
            Console.WriteLine("Start method HasBeenAdded");

            try
            {
                Console.WriteLine($"Checking for song with Title: {title} and Artist: {artist}");

                var result = _context.Songs
                    .Any(x => x.Title == title && x.Artist == artist);

                Console.WriteLine("Query executed successfully");
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
                if (e.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {e.InnerException.Message}");
                }
                throw;
            }


        }

        public async Task<IEnumerable<Song>> GetSongByGenre(string genre)
        {
            var result =  _context.Songs.Where(x => x.Genres.Contains(genre) );
            return result.AsEnumerable();
        }

        public async Task<IEnumerable<Song>> GetSongsByGenreAndUser(string genre, Guid userId)
        {
            var result =  _context.UserPlayedSongs
                .Where(x => x.UserId == userId)
                .Select(x=>x.Song)
                .Where(song=>song.Genres.Contains(genre));
            return result.AsEnumerable();
        }

        public async Task<IEnumerable<Song>> GetSongsByUser(Guid userId)
        {
            var result = _context.UserPlayedSongs
                .Where(x => x.UserId == userId)
                .Select(x => x.Song);
            return result.AsEnumerable();
        }
    }
}
