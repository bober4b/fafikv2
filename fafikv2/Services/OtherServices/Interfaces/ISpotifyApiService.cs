
namespace Fafikv2.Services.OtherServices.Interfaces
{
    public interface ISpotifyApiService
    {
        public Task<string?[]> GetGenresOfTrackAsync(string trackName, string artistName);
    }
}
