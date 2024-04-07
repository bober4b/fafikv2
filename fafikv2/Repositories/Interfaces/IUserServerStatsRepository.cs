﻿using Fafikv2.Data.Models;
namespace Fafikv2.Repositories.Interfaces
{
    public interface IUserServerStatsRepository
    {
        public Task AddUserServerStats(UserServerStats userServerStats);
        public Task UpdateUserServerStats(UserServerStats userServerStats);
        public Task DeleteUserServerStats(UserServerStats userServerStats);

        public IEnumerable<UserServerStats> GetAll();
    }
}