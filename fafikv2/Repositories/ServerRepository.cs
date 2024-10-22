using Fafikv2.Data.DataContext;
using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;



namespace Fafikv2.Repositories
{
    public class ServerRepository : IServerRepository
    {
        private readonly DiscordBotDbContext _context;

        public ServerRepository(DiscordBotDbContext context)
        {
            _context = context;
        }

        public Task AddServer(Server server)
        {
            _context.Servers.Add(server);
            Console.WriteLine("xD");
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while saving server data: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }

            Console.WriteLine("xD");
            return Task.CompletedTask;
        }

        public Task DeleteServer(Server server)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Server> GetAll()
        {
            var result = _context.Servers;

            return result;
        }


    }
}