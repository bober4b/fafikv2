using Fafikv2.Data.Models;

namespace Fafikv2.Services.dbServices.Interfaces
{
    public interface IServerConfigService
    {
        public Task AddServerConfig(ServerConfig serverConfig);
        public Task RemoveServerConfig(ServerConfig serverConfig);
        public Task UpdateServerConfig(ServerConfig serverConfig);
        public Task<ServerConfig?> GetServerConfig(Guid server);
        public Task EnableBans(Guid server);
        public Task DisableBans(Guid server);
        public Task EnableKicks(Guid server);
        public Task DisableKicks(Guid server);
        public Task EnableAutoModerator(Guid server);
        public Task DisableAutoModerator(Guid server);
        public Task EnableAutoPlay(Guid server);
        public Task DisableAutoPlay(Guid server);
        public Task<bool> IsAutoPlayEnable(Guid server);

    }

    
}
