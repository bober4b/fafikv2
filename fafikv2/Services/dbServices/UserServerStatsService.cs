using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;
using Fafikv2.Services.dbServices.Interfaces;

namespace Fafikv2.Services.dbServices
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
            if (userServerStats.ServerUsers != null)
            {
                var newstats = await _userServerRepository.GetUserstatsByUserAndServerId(userServerStats.ServerUsers.UserId,
                    userServerStats.ServerUsers.ServerId).ConfigureAwait(false);
                

                if (newstats != null)
                {
                    return;
                }
                await _userServerRepository.AddUserServerStats(userServerStats).ConfigureAwait(false);
            }

            

            await Task.CompletedTask.ConfigureAwait(false);
        }

        public Task UpdateUserServerStats(UserServerStats userServerStats)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateUserMessageServerCount(Guid userId, Guid serverId)
        {
            var user = await _userServerRepository.GetUserstatsByUserAndServerId(userId, serverId).ConfigureAwait(false)
                       ?? throw new InvalidOperationException("user not found");
            

            user.MessagesCountServer++;

            await _userServerRepository.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateUserBotInteractionsServerCount(Guid userId, Guid serverId)
        {
            var user = await _userServerRepository.GetUserstatsByUserAndServerId(userId, serverId).ConfigureAwait(false)
                       ?? throw new InvalidOperationException("user not found");

            user.BotInteractionServer++;

            await _userServerRepository.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<UserServerStats?> GetUserStats(Guid userId, Guid serverId)
        {
           return await _userServerRepository.GetUserstatsByUserAndServerId(userId, serverId).ConfigureAwait(false);
        }
    }
}
