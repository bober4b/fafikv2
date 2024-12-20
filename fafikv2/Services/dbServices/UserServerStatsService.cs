﻿using Fafikv2.Data.Models;
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
                var newStats = await _userServerRepository.GetUserStatsByUserAndServerId(userServerStats.ServerUsers.UserId,
                    userServerStats.ServerUsers.ServerId);


                if (newStats != null)
                {
                    return;
                }
                await _userServerRepository.AddUserServerStats(userServerStats);
            }

            await Task.CompletedTask;
        }

        public Task UpdateUserServerStats(UserServerStats userServerStats)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateUserServerStats(Guid userId, Guid serverId, string newDisplayName)
        => await _userServerRepository.UpdateUserServer(userId, serverId, newDisplayName);
        
        public async Task UpdateUserMessageServerCount(Guid userId, Guid serverId)
        {
            var user = await _userServerRepository.GetUserStatsByUserAndServerId(userId, serverId)
                       ?? throw new InvalidOperationException("user not found");


            user.MessagesCountServer++;

            await _userServerRepository.SaveChangesAsync();
            Console.WriteLine(user.DisplayName + " stats: " + user.BotInteractionServer + " " + user.MessagesCountServer);
        }

        public async Task UpdateUserBotInteractionsServerCount(Guid userId, Guid serverId)
        {
            var user = await _userServerRepository.GetUserStatsByUserAndServerId(userId, serverId)
                       ?? throw new InvalidOperationException("user not found");

            user.BotInteractionServer++;

            await _userServerRepository.SaveChangesAsync();
            Console.WriteLine(user.DisplayName + " stats: " + user.BotInteractionServer + " " + user.MessagesCountServer);
        }

        public async Task<UserServerStats?> GetUserStats(Guid userId, Guid serverId)
        => await _userServerRepository.GetUserStatsByUserAndServerId(userId, serverId);

        public async Task<UserServerStats?> AsNoTracking(Guid userId, Guid serverId)
            => await _userServerRepository.GetOnlyToRead(userId, serverId);

        public async Task AddPenalty(Guid userId, Guid serverId)
        {
            var user = await _userServerRepository
                .GetUserStatsByUserAndServerId(userId, serverId);

            if(user== null) return;

            user.LastPenaltyDate = DateTime.Now;
            user.Penalties++;
            try
            {
                await _userServerRepository.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task<IEnumerable<UserServerStats>> GetUsersStatsByServer(Guid serverId)
        => await _userServerRepository.GetUsersStatsByServer(serverId);

        public async Task<IEnumerable<UserServerStats>> GetUserStatsByServerOnlyToRead(Guid serverId)
            => await _userServerRepository.GetUserServerStatsToRead(serverId);

    }
}
