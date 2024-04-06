using Fafikv2.Data.Models;
using Fafikv2.Data.DataContext;
using Fafikv2.Repositories.Interfaces;


namespace Fafikv2.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DiscordBotDbContext _Context;

        public UserRepository(DiscordBotDbContext context)
        {
            _Context=context;
        }

        public Task AddUser(User user)
        {
            _Context.Users.Add(user);
            _Context.SaveChanges();
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
            var result = _Context.Users;
            return result;
        }

    }
}
