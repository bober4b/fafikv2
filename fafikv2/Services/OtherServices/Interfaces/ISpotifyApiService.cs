
namespace Fafikv2.Services.OtherServices.Interfaces
{
    public interface ISpotifyApiService
    {
        public Task<string?[]> GetGenresOfTrack(string query);
        public Task<string> GetRecommendationsBasedOnInput(string userInput);
        public Task<string> GetRecommendationBasenOnGenre(string userInput);
    }
}
