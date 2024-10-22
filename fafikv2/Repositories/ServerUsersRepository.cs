using Fafikv2.Data.DataContext;
using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;

namespace Fafikv2.Repositories
{
    public class ServerUsersRepository : IServerUsersRepository
    {
        private readonly DiscordBotDbContext _context;

        public ServerUsersRepository(DiscordBotDbContext context)
        {
            _context = context;
        }


        public Task AddServerUser(ServerUsers serverUsers)
        {
            _context.ServerUsers.Add(serverUsers);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error AddServerUser" + ex.Message);
            }
            //_context.SaveChanges();

            return Task.CompletedTask;
        }

        public IEnumerable<ServerUsers> GetAll()
        {
            var result = _context.ServerUsers;
            return result;
        }
    }
}
