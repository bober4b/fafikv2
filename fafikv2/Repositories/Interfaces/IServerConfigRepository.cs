using Fafikv2.Data.Models;
namespace Fafikv2.Repositories.Interfaces
{
    public interface IServerConfigRepository
    {
        public Task AddServerConfig(ServerConfig serverConfig);
        public Task UpdateServerConfig(ServerConfig serverConfig);
        public Task DeleteServerConfig(ServerConfig serverConfig);
        public IEnumerable<ServerConfig> GetAll();

    }
}
