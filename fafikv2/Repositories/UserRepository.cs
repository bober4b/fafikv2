using Fafikv2.Data.Models;
using Fafikv2.Data.DataContext;
using Fafikv2.Repositories.Interfaces;

namespace Fafikv2.Repositories
{
    public class UserRepository : IUserRepository
    {
        public DiscordBotDbContext _Context;

        public UserRepository(DiscordBotDbContext context)
        {
            context = _Context;
        }

        public Task AddUser(User user)
        {
            throw new NotImplementedException();
        }

        public Task DeleteUser(User user)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUser(User user)
        {
            throw new NotImplementedException();
        }

    }
}
