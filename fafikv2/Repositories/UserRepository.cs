using Fafikv2.Data.Models;
using Fafikv2.Data.DataContext;
using Fafikv2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


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

        public async Task<User?> GetUserById(Guid userId)
        {
            
            return await _context
                .Users
                .FirstOrDefaultAsync(x => x.Id == userId) ;
            
        }

        public async Task UpdateUser(User user)
        {
            var existingUser = _context.Users.FirstOrDefault(x => x.Id == user.Id) ?? throw new InvalidOperationException("user not found");
            
            
            var properties = typeof(User)
                .GetProperties()
                .Where(p => p.GetValue(user) != null);

            foreach (var property in properties)
            {
                property.SetValue(existingUser, property.GetValue(user));
            }

            await _context.SaveChangesAsync() ;
        }

        public IEnumerable<User> GetAll()
        {
            var result = _context.Users;
            return result;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync() ;
        }
    }
}
