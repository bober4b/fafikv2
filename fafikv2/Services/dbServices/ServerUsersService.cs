using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;
using Fafikv2.Services.dbServices.Interfaces;

namespace Fafikv2.Services.dbServices
{
    public class ServerUsersService : IServerUsersService
    {
        private readonly IServerUsersRepository _serverUsersRepository;

        public ServerUsersService(IServerUsersRepository serverUsersRepository)
        {
            _serverUsersRepository = serverUsersRepository;
        }


        public async Task AddServerUsers(ServerUsers serverUsers)
        {
            var userExistInServer = _serverUsersRepository
                .GetAll()
                .FirstOrDefault(x => x.ServerId == serverUsers.ServerId && x.UserId == serverUsers.UserId);
            if (userExistInServer != null)
            {
                return;
            }

            _serverUsersRepository.AddServerUser(serverUsers);
        }

        public Task UpdateServerUsers(ServerUsers serverUsers)
        {
            throw new NotImplementedException();
        }
    }
}
