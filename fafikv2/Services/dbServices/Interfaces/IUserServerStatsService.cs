using Fafikv2.Data.Models;

namespace Fafikv2.Services.dbServices.Interfaces
{
    public interface IUserServerStatsService
    {
        public Task AddUserServerStats(UserServerStats userServerStats);
        public Task UpdateUserServerStats(UserServerStats userServerStats);
        public Task UpdateUserServerStats(Guid userId, Guid serverId, string newDisplayName);
        public Task UpdateUserMessageServerCount(Guid userId, Guid serverId);
        public Task UpdateUserBotInteractionsServerCount(Guid userId, Guid serverId);
        public Task<UserServerStats?> GetUserStats(Guid userId, Guid serverId);
        public Task AddPenalty(Guid userId, Guid serverId);
        public Task<IEnumerable<UserServerStats>> GetUsersStatsByServer(Guid serverId);

    }
}
