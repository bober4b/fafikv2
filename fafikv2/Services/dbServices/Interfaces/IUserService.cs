using Fafikv2.Data.Models;

namespace Fafikv2.Services.dbServices.Interfaces
{
    public interface IUserService
    {
        public Task AddUser(User user);
        public Task UpdateUser(User user);
        public Task UpdateUser(Guid guid, string user);
        public Task UpdateUserMessageCount(Guid userId);
        public Task UpdateUserBotInteractionsCount(Guid userId);
        public Task<User?> GetUser(Guid userId);


    }
}
