using Fafikv2.Data.Models;
using Fafikv2.Data.DataContext;
using Fafikv2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

            try
            {
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException.Message);
                throw;
            }
            //_context.SaveChanges();
            return Task.CompletedTask;
        }

        public async Task UpdateUserServerStats(UserServerStats userServerStats)
        {
            _context.ServerUsersStats.Update(userServerStats);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public Task DeleteUserServerStats(UserServerStats userServerStats)
        {
            throw new NotImplementedException();
        }

        public async Task<UserServerStats?> GetUserStatsByUserAndServerId(Guid userId, Guid serverId)
        {
            var serverUsers = _context
                .ServerUsers
                .FirstOrDefault(x => x.ServerId == serverId && x.UserId == userId);

            if (serverUsers != null)
            {
                var result= _context.ServerUsersStats.FirstOrDefault(x => serverUsers != null && x.ServerUserId == serverUsers.Id);

                if (result != null)
                {
                    return result;
                }

            }

            return null;
        }

        public IEnumerable<UserServerStats> GetAll()
        {
            var result = _context.ServerUsersStats;
            return result;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        
    }
}
