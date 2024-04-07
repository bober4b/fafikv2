using Fafikv2.Data.Models;
using Fafikv2.Data.DataContext;
using Fafikv2.Repositories.Interfaces;

namespace Fafikv2.Repositories
{
    public class UserServerStatsRepository : IUserServerStatsRepository
    {
        private readonly DiscordBotDbContext _context;

        public UserServerStatsRepository(DiscordBotDbContext context)
        {
            _context = context;
        }
        public Task AddUserServerStats(UserServerStats userServerStats)
        {
            _context.ServerUsersStats.Add(userServerStats);
            _context.SaveChanges();
            return Task.CompletedTask;
        }

        public Task UpdateUserServerStats(UserServerStats userServerStats)
        {
            throw new NotImplementedException();
        }

        public Task DeleteUserServerStats(UserServerStats userServerStats)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserServerStats> GetAll()
        {
            var result = _context.ServerUsersStats;
            return result;
        }
    }
}
