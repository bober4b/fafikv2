using Fafikv2.Data.Models;

namespace Fafikv2.Services.dbServices.Interfaces
{
    public interface IUserPlayedSongsService
    {
        public Task Add(UserPlayedSong userPlayedSong);

    }
}
