using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;
using Fafikv2.Services.dbServices.Interfaces;

namespace Fafikv2.Services.dbServices
{
    public class ServerConfigService : IServerConfigService
    {
        private readonly IServerConfigRepository _serverConfigRepository;

        public ServerConfigService(IServerConfigRepository serverConfigRepository)
        {
            _serverConfigRepository = serverConfigRepository;
        }

        public async Task AddServerConfig(ServerConfig serverConfig)
        {
            var newConfig = _serverConfigRepository
                .GetAll()
                .FirstOrDefault(x => x.Id == serverConfig.Id || x.ServerId == serverConfig.ServerId);
            if (newConfig != null)
            {
                return;
            }


            await _serverConfigRepository.AddServerConfig(serverConfig);

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

        public Task<ServerConfig?> GetServerConfig(Guid server)
        {
            var result = _serverConfigRepository
                .GetAll()
                .FirstOrDefault(x => x.ServerId == server);

            return Task.FromResult(result);
        }

        public async Task EnableBans(Guid server)
        {
            await _serverConfigRepository.EnableDisableBans(server, true);
        }

        public async Task DisableBans(Guid server)
        {
            await _serverConfigRepository.EnableDisableBans(server, false);
        }

        public async Task EnableKicks(Guid server)
        {
            await _serverConfigRepository.EnableDisableKicks(server, true);
        }

        public async Task DisableKicks(Guid server)
        {
            await _serverConfigRepository.EnableDisableKicks(server, false);
        }

        public async Task EnableAutoModerator(Guid server)
        {
            await _serverConfigRepository.EnableDisableAutoModerator(server, true);
        }

        public async Task DisableAutoModerator(Guid server)
        {
            await _serverConfigRepository.EnableDisableAutoModerator(server, false);
        }

        public async Task EnableAutoPlay(Guid server)
        {
            await _serverConfigRepository.EnableDisableAutoPlay(server, true);
        }

        public async Task DisableAutoPlay(Guid server)
        {
            await _serverConfigRepository.EnableDisableAutoPlay(server, false);

        }

        public async Task<bool> IsAutoPlayEnable(Guid server)
        {
            return await _serverConfigRepository.IsAutoPlayEnable(server);
        }
    }
}
