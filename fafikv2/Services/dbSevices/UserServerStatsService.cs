using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;
using Fafikv2.Services.dbSevices.Interfaces;

namespace Fafikv2.Services.dbSevices
{
    public class UserServerStatsService : IUserServerStatsService
    {
        private readonly IUserServerStatsRepository _userServerRepository;

        public UserServerStatsService(IUserServerStatsRepository userServerStatsRepository)
        {
            _userServerRepository = userServerStatsRepository;
        }

        public async Task AddUserServerStats(UserServerStats userServerStats)
        {
            var newstats = _userServerRepository
                .GetAll()
                .FirstOrDefault(x => x.Id == userServerStats.Id);
            if (newstats != null)
            {
                return;
            }

            _userServerRepository.AddUserServerStats(userServerStats);

            await Task.CompletedTask;

        }

        public Task UpdateUserServerStats(UserServerStats userServerStats)
        {
            throw new NotImplementedException();
        }
    }
}
