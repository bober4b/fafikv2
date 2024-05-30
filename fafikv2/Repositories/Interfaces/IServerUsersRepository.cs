using Fafikv2.Data.Models;

namespace Fafikv2.Repositories.Interfaces
{
    public interface IServerUsersRepository
    {
        public Task AddServerUser(ServerUsers serverUsers);
        public IEnumerable<ServerUsers> GetAll();
    }
}
