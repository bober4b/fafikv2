﻿using Fafikv2.Data.DataContext;
using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fafikv2.Repositories
{
    public class UserServerStatsRepository : IUserServerStatsRepository
    {
        private readonly DiscordBotDbContext _context;

        public UserServerStatsRepository(DiscordBotDbContext context)
        {
            _context = context;
        }
        public Task AddUserServerStats(UserServerStats userServerStats)
        {
            _context.ServerUsersStats.Add(userServerStats);

            try
            {
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException?.Message);
                throw;
            }
            //_context.SaveChanges();
            return Task.CompletedTask;
        }

        public async Task UpdateUserServerStats(UserServerStats userServerStats)
        {
            _context.ServerUsersStats.Update(userServerStats);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserServer(Guid userId, Guid serverId, string newDisplayName)
        {
            var userServerStats = await GetUserStatsByUserAndServerId(userId, serverId);

            if (userServerStats == null) return;
            
            userServerStats.DisplayName = newDisplayName;
            await _context.SaveChangesAsync();
            await _context.Entry(userServerStats).ReloadAsync();

        }

        public Task DeleteUserServerStats(UserServerStats userServerStats)
        {
            throw new NotImplementedException();
        }

        public Task<UserServerStats?> GetUserStatsByUserAndServerId(Guid userId, Guid serverId)
        {
            var serverUsers = _context
                .ServerUsers
                .FirstOrDefault(x => x.ServerId == serverId && x.UserId == userId);

            
            var result = _context.ServerUsersStats.FirstOrDefault(x => x.ServerUserId == serverUsers!.Id);

            return Task.FromResult(result);
            
            
        }

        public Task<UserServerStats?> GetOnlyToRead(Guid userId, Guid serverId)
        {

            var serverUsers = _context
                .ServerUsers
                .FirstOrDefault(x => x.ServerId == serverId && x.UserId == userId);

            var noTracking = _context.ServerUsersStats.AsNoTracking();

             var result =noTracking.FirstOrDefault(x => x.ServerUserId == serverUsers!.Id);

            return Task.FromResult(result);
        }

        public IEnumerable<UserServerStats> GetAll()
        {
            var result = _context.ServerUsersStats;
            return result;
        }

        public async Task<IEnumerable<UserServerStats>> GetUsersStatsByServer(Guid serverId)
        {

            var serverUsers = _context.ServerUsers.Where(x => x.ServerId == serverId);
            var result = await _context.ServerUsersStats
                .Where(uss => serverUsers.Any(su => su.Id == uss.ServerUserId))
                .OrderByDescending(stats => stats.MessagesCountServer)
                .ToListAsync();
            return result;
        }

        public async Task<IEnumerable<UserServerStats>> GetUserServerStatsToRead(Guid serverId)
        {
            var serverUsers = _context.ServerUsers.Where(x => x.ServerId == serverId);

            var noTracking = _context.ServerUsersStats.AsNoTracking();
            var result =await noTracking.Where(uss => serverUsers.Any(su => su.Id == uss.ServerUserId))
                .OrderByDescending(stats => stats.MessagesCountServer)
                .ToListAsync();

            return result;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
