using Fafikv2.Data.DataContext;
using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fafikv2.Repositories
{
    public class UserPlayedSongsRepository : IUserPlayedSongsRepository
    {
        private readonly DiscordBotDbContext _context;

        public UserPlayedSongsRepository(DiscordBotDbContext context)
        {
            _context = context;
        }
        public async Task Add(UserPlayedSong userPlayedSong)
        {
            
            try
            {
                Console.WriteLine("Adding UserPlayedSong to the database.");

                // Dodanie obiektu do kontekstu
                await _context.UserPlayedSongs.AddAsync(userPlayedSong) ;
                Console.WriteLine("UserPlayedSong added to the context.");

                // Zapisanie zmian do bazy danych
                await _context.SaveChangesAsync() ;
                Console.WriteLine("Changes saved to the database.");
            }
            catch (Exception ex)
            {
                // Obsługa wyjątków
                Console.WriteLine($"An error occurred while adding UserPlayedSong to the database: {ex.Message}");

                // Sprawdzenie szczegółów wyjątku
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                throw; // Ponowne rzucenie wyjątku
            }
        }

        public async Task<bool> HasBeenAdded(UserPlayedSong userPlayedSong)
        {
            var result=await _context.UserPlayedSongs
                .AnyAsync(x=>x.User != null && userPlayedSong.Song != null && x.Song != null && x.Song.Title==userPlayedSong.Song.Title && x.Song.Artist ==userPlayedSong.Song.Artist && x.User.Id==userPlayedSong.UserId);
            return result;
        }
    }
}
