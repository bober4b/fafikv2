using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;
using Fafikv2.Services.dbServices.Interfaces;

namespace Fafikv2.Services.dbServices
{
    internal class ServerService : IServerService
    {
        private readonly IServerRepository _serverRepository;


        public ServerService(IServerRepository serverRepository)
        {
            _serverRepository = serverRepository;
        }

        public async Task AddServer(Server server)
        {
            var newServer = _serverRepository
                .GetAll()
                .FirstOrDefault(x => x.Id == server.Id);
            if (newServer != null)
            {
                return;
            }
            _serverRepository.AddServer(server);

            await Task.CompletedTask;

        }

        public Task UpdateServer(Server server)
        {
            throw new NotImplementedException();
        }
    }
}
