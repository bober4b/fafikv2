using Fafikv2.Data.Models;
using Fafikv2.Data.DataContext;
using Fafikv2.Repositories.Interfaces;


namespace Fafikv2.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DiscordBotDbContext _context;

        public UserRepository(DiscordBotDbContext context)
        {
            _context=context;
        }

        public Task AddUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return Task.CompletedTask;

        }

        public Task DeleteUser(User user)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUser(User user)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<User> GetAll()
        {
            var result = _context.Users;
            return result;
        }

    }
}
