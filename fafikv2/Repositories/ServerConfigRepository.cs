﻿using Fafikv2.Data.Models;
using Fafikv2.Data.DataContext;
using Fafikv2.Repositories.Interfaces;


namespace Fafikv2.Repositories
{
    public class ServerConfigRepository : IServerConfigRepository
    {
        private readonly DiscordBotDbContext _context;

        public ServerConfigRepository(DiscordBotDbContext context)
        {
            _context =context;
        }

        public Task AddServerConfig(ServerConfig serverConfig)
        {
            _context.ServerConfigs.Add(serverConfig);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);
            }
           
            return Task.CompletedTask;
        }

        public Task UpdateServerConfig(ServerConfig serverConfig)
        {
            throw new NotImplementedException();
        }

        public Task DeleteServerConfig(ServerConfig serverConfig)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ServerConfig> GetAll()
        {
            var result = _context.ServerConfigs;
            return result;
        }
    }
}
