﻿using Fafikv2.Data.Models;
namespace Fafikv2.Repositories.Interfaces
{
    public interface IUserServerStatsRepository
    {
        public Task AddUserServerStats(UserServerStats userServerStats);
        public Task UpdateUserServerStats(UserServerStats userServerStats);
        public Task DeleteUserServerStats(UserServerStats userServerStats);
        public Task<UserServerStats> GetUserStatsByUserAndServerId(Guid userId, Guid serverId);
        public IEnumerable<UserServerStats> GetAll();
        public Task<IEnumerable<UserServerStats>> GetUsersStatsByServer(Guid serverId);
        public Task SaveChangesAsync();
    }
}
