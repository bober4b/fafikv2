using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;
using Fafikv2.Services.dbServices.Interfaces;
using Microsoft.EntityFrameworkCore;

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
                var newstats = await _userServerRepository.GetUserStatsByUserAndServerId(userServerStats.ServerUsers.UserId,
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
            var user = await _userServerRepository.GetUserStatsByUserAndServerId(userId, serverId).ConfigureAwait(false)
                       ?? throw new InvalidOperationException("user not found");
            

            user.MessagesCountServer++;

            await _userServerRepository.SaveChangesAsync().ConfigureAwait(false);
            Console.WriteLine(user.DisplayName + " stats: " + user.BotInteractionServer + " " + user.MessagesCountServer);
        }

        public async Task UpdateUserBotInteractionsServerCount(Guid userId, Guid serverId)
        {
            var user = await _userServerRepository.GetUserStatsByUserAndServerId(userId, serverId).ConfigureAwait(false)
                       ?? throw new InvalidOperationException("user not found");

            user.BotInteractionServer++;

            await _userServerRepository.SaveChangesAsync().ConfigureAwait(false);
            Console.WriteLine(user.DisplayName+" stats: "+user.BotInteractionServer+" "+user.MessagesCountServer);
        }

        public async Task<UserServerStats?> GetUserStats(Guid userId, Guid serverId)
        {
           return await _userServerRepository.GetUserStatsByUserAndServerId(userId, serverId).ConfigureAwait(false);
        }

        public async Task AddPenalty(Guid userId, Guid serverId)
        {
            var user = await _userServerRepository
                .GetUserStatsByUserAndServerId(userId, serverId)
                .ConfigureAwait(false);
            if (user != null)
            {
                user.LastPenaltyDate = DateTime.Now;
                user.Penalties++;
                try
                {
                    await _userServerRepository.SaveChangesAsync().ConfigureAwait(false);
                    Console.WriteLine("tak?");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
                //await _userServerRepository.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
