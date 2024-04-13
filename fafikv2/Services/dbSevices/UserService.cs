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
            var newuser = _userRepository
                .GetAll()
                .FirstOrDefault(x => x.Id == user.Id);
            if (newuser != null)
            {
                return;
            }
            await _userRepository.AddUser(user).ConfigureAwait(false);
            

            await Task.CompletedTask.ConfigureAwait(false);
        }

        public Task UpdateUser(User user)
        {
            return Task.CompletedTask;
        }

        public async Task UpdateUserMessageCount(Guid userId)
        {
            var user = await _userRepository.GetUserById(userId).ConfigureAwait(false) 
                       ?? throw new InvalidOperationException("user not found");

            
            user.MessagesCountGlobal++;

            await _userRepository.SaveChangesAsync().ConfigureAwait(false);



        }

        public async Task UpdateUSerBotInteractionsCount(Guid userId)
        {
            var user = await _userRepository.GetUserById(userId).ConfigureAwait(false)
                       ?? throw new InvalidOperationException("user not found");

            user.BotInteractionGlobal++;

            await _userRepository.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
