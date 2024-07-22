using Fafikv2.Data.Models;

namespace Fafikv2.Repositories.Interfaces
{
    public interface IUserPlayedSongsRepository
    {
        public Task Add(UserPlayedSong userPlayedSong);
        public Task<bool> HasBeenAdded(UserPlayedSong userPlayedSong);

    }
}
