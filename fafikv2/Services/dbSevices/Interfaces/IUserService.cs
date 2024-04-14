using Fafikv2.Data.Models;

namespace Fafikv2.Services.dbSevices.Interfaces
{
    public interface IUserService
    {
        public Task AddUser(User  user);
        public Task UpdateUser(User user);
        public Task UpdateUserMessageCount(Guid userId);
        public Task UpdateUserBotInteractionsCount(Guid userId);
        

    }
}
