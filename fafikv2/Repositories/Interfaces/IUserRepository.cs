using Fafikv2.Data.Models;
namespace Fafikv2.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task AddUser(User user);
        public Task UpdateUser(User user);
        public Task DeleteUser(User user);
        public Task<User?> GetUserById(Guid userId);
        public IEnumerable<User> GetAll();

        public Task SaveChangesAsync();

    }
}
