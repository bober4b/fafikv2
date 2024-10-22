using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;
using Fafikv2.Services.dbServices.Interfaces;

namespace Fafikv2.Services.dbServices
{
    internal class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;


        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task AddUser(User user)
        {
            var newUser = _userRepository
                .GetAll()
                .FirstOrDefault(x => x.Id == user.Id);
            if (newUser != null)
            {
                return;
            }
            await _userRepository.AddUser(user);


            await Task.CompletedTask;
        }

        public Task UpdateUser(User user)
        {
            return Task.CompletedTask;
        }

        public async Task UpdateUserMessageCount(Guid userId)
        {
            var user = await _userRepository.GetUserById(userId)
                       ?? throw new InvalidOperationException("user not found");


            user.MessagesCountGlobal++;

            await _userRepository.SaveChangesAsync();



        }

        public async Task UpdateUserBotInteractionsCount(Guid userId)
        {
            var user = await _userRepository.GetUserById(userId)
                       ?? throw new InvalidOperationException("user not found");

            user.BotInteractionGlobal++;

            await _userRepository.SaveChangesAsync();
        }

        public Task<User?> GetUser(Guid userId)
        {
            return _userRepository.GetUserById(userId);
        }
    }
}
