using Fafikv2.Data.Models;

namespace Fafikv2.Services.dbServices.Interfaces
{
    public interface IServerConfigService
    {
        public Task AddServerConfig(ServerConfig serverConfig);
        public Task RemoveServerConfig(ServerConfig serverConfig);
        public Task UpdateServerConfig(ServerConfig serverConfig);
        public Task<ServerConfig> GetServerConfig(Guid server);
    }

    
}
