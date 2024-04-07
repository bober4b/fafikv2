﻿using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;
using Fafikv2.Services.dbSevices.Interfaces;

namespace Fafikv2.Services.dbSevices
{
    public class ServerConfigService : IServerConfigService
    {
        private readonly IServerConfigRepository _serverConfigRepository;

        public ServerConfigService(IServerConfigRepository serverConfigRepository)
        {
            _serverConfigRepository= serverConfigRepository;
        }

        public async Task AddServerConfig(ServerConfig serverConfig)
        {
            var newConfig = _serverConfigRepository
                .GetAll()
                .FirstOrDefault(x => x.Id == serverConfig.Id);
            if (newConfig != null)
            {
                return;
            }

            _serverConfigRepository.AddServerConfig(serverConfig);

            await Task.CompletedTask;
        }

        public Task RemoveServerConfig(ServerConfig serverConfig)
        {
            throw new NotImplementedException();
        }

        public Task UpdateServerConfig(ServerConfig serverConfig)
        {
            throw new NotImplementedException();
        }
    }
}