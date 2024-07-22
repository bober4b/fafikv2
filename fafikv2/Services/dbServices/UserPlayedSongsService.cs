using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;
using Fafikv2.Services.dbServices.Interfaces;

namespace Fafikv2.Services.dbServices
{
    public class UserPlayedSongsService : IUserPlayedSongsService
    {
        private readonly IUserPlayedSongsRepository _userPlayedSongsRepository;

        public UserPlayedSongsService(IUserPlayedSongsRepository userPlayedSongsRepository)
        {
            _userPlayedSongsRepository = userPlayedSongsRepository;
        }

        public async Task Add(UserPlayedSong userPlayedSong)
        {
            var added = await _userPlayedSongsRepository.HasBeenAdded(userPlayedSong).ConfigureAwait(false);
            if (added) return;
            await _userPlayedSongsRepository.Add(userPlayedSong).ConfigureAwait(false);
        }
    }
}
