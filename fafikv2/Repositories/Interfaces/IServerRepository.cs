using Fafikv2.Data.Models;

namespace Fafikv2.Repositories.Interfaces
{
    public interface IServerRepository
    {
        public Task AddServer(Server server);
        public Task DeleteServer(Server server);
        public IEnumerable<Server> GetAll();

    }
}
