using Fafikv2.Data.Models;

namespace Fafikv2.Services.dbServices.Interfaces
{
    public interface IServerUsersService
    {
        public Task AddServerUsers(ServerUsers serverUsers);
        public Task UpdateServerUsers(ServerUsers serverUsers);
    }
}
