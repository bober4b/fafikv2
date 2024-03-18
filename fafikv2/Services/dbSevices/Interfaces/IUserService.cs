using Fafikv2.Data.Models;

namespace Fafikv2.Services.dbSevices.Interfaces
{
    public interface IUserService
    {
        public Task AddUser(User  user);
        public Task UpdateUser(User user);

    }
}
