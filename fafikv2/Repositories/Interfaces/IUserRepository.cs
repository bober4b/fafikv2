using Fafikv2.Data.Models;
namespace Fafikv2.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task AddUser(User user);
        public Task UpdateUser(User user);
        public Task DeleteUser(User user);
        public IEnumerable<User> GetAll();

    }
}
