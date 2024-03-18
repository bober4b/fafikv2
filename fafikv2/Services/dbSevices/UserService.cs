using System.Runtime.InteropServices;
using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;
using Fafikv2.Services.dbSevices.Interfaces;

namespace Fafikv2.Services.dbSevices
{
    internal class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        

        public UserService(IUserRepository userRepository)
        {
            _userRepository= userRepository;
        }

        public async Task AddUser(User user)
        {
            
            
                var newuser = _userRepository.GetAll().FirstOrDefault(x => x.Id == user.Id);
                if (newuser != null)
                {
                    return;
                }
                _userRepository.AddUser(user);
            

            await Task.CompletedTask;
        }

        public Task UpdateUser(User user)
        {
            return Task.CompletedTask;
        }
    }
}
