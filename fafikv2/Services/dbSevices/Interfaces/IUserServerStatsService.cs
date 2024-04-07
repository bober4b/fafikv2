using Fafikv2.Data.Models;

namespace Fafikv2.Services.dbSevices.Interfaces
{
    public interface IUserServerStatsService
    {
        public Task AddUserServerStats(UserServerStats userServerStats);
        public Task UpdateUserServerStats(UserServerStats userServerStats);

    }
}
