using Fafikv2.Data.Models;
namespace Fafikv2.Services.dbServices.Interfaces
{
    public interface IServerService
    {
        public Task AddServer(Server server);
        public Task UpdateServer(Server server);
    }
}
