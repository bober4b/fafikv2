﻿using Fafikv2.Data.Models;

namespace Fafikv2.Services.dbSevices.Interfaces
{
    public interface IUserServerStatsService
    {
        public Task AddUserServerStats(UserServerStats userServerStats);
        public Task UpdateUserServerStats(UserServerStats userServerStats);
        public Task UpdateUserMessageServerCount(Guid userId, Guid serverId);
        public Task UpdateUserBotInteractionsServerCount(Guid userId, Guid serverId);

    }
}
