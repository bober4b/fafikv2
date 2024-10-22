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
            _context = context;
        }

        public async Task AddSong(Song song)
        {
            _context.Songs.Add(song);
            await _context.SaveChangesAsync();
        }


        public Task RemoveSong(Song song)
        {
            throw new NotImplementedException();
        }

        public async Task<Song?> Get(string? title, string? artist)
        {
            var result = await _context.Songs
                .FirstOrDefaultAsync(x => x.Artist == artist && x.Title == title);
            return result;
        }

        public Task<bool> HasBeenAdded(string? title, string? artist)
        {
            Console.WriteLine("Start method HasBeenAdded");

            try
            {
                Console.WriteLine($"Checking for song with Title: {title} and Artist: {artist}");

                var result = _context.Songs
                    .Any(x => x.Title == title && x.Artist == artist);

                Console.WriteLine("Query executed successfully");
                return Task.FromResult(result);
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

        public async Task<IEnumerable<Song>> GetSongByGenre(string? genre)
        {
            if (string.IsNullOrEmpty(genre))
            {
                return Enumerable.Empty<Song>();
            }

            // Surowe zapytanie SQL
            var genres = genre.Split(',')
                .Select(g => g.Trim())
                .ToArray(); // Ensure array of genres

            // Convert genres into a SQL-friendly format for `IN` clause
            var genreParameter = string.Join(",", genres.Select(g => $"'{g}'")); // SQL injection safe, as genres are known to be strings

            // Optimized query that matches any genre in the list
            var query = $@"
                        SELECT DISTINCT  s.*
                        FROM UserPlayedSongs ups
                        JOIN Songs s ON ups.SongId = s.Id
                        CROSS APPLY STRING_SPLIT(s.Genres, ',') AS genre_split
                        WHERE LTRIM(RTRIM(genre_split.value)) IN ({genreParameter})";

            var result = await _context.Songs
                .FromSqlRaw(query, genre)
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<Song?>> GetSongsByGenreAndUser(string? genre, Guid userId)
        {
            if (string.IsNullOrEmpty(genre))
            {
                return Enumerable.Empty<Song?>();
            }

            // Split genres and clean them up
            var genres = genre.Split(',')
                .Select(g => g.Trim())
                .ToArray(); // Ensure array of genres

            // Convert genres into a SQL-friendly format for `IN` clause
            var genreParameter = string.Join(",", genres.Select(g => $"'{g}'")); // SQL injection safe, as genres are known to be strings

            // Optimized query that matches any genre in the list
            var query = $@"
                        SELECT DISTINCT  s.*
                        FROM UserPlayedSongs ups
                        JOIN Songs s ON ups.SongId = s.Id
                        CROSS APPLY STRING_SPLIT(s.Genres, ',') AS genre_split
                        WHERE ups.UserId = {{0}}
                        AND LTRIM(RTRIM(genre_split.value)) IN ({genreParameter})";

            // Execute the query once, matching all genres
            var result = await _context.Songs
                .FromSqlRaw(query, userId)
                .ToListAsync();

            return result;
        }

        public Task<IEnumerable<Song?>> GetSongsByUser(Guid userId)
        {
            var result = _context.UserPlayedSongs
                .Where(x => x.UserId == userId)
                .Select(x => x.Song);
            return Task.FromResult(result.AsEnumerable());
        }

        public Task<IEnumerable<Song>?> GetAll()
        {
            return Task.FromResult<IEnumerable<Song>?>(_context.Songs);
        }
    }
}
