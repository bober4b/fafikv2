using Fafikv2.Data.Models;

namespace Fafikv2.Services.dbSevices.Interfaces
{
    public interface IServerConfigService
    {
        public Task AddServerConfig(ServerConfig serverConfig);
        public Task RemoveServerConfig(ServerConfig serverConfig);
        public Task UpdateServerConfig(ServerConfig serverConfig);
    }
}
