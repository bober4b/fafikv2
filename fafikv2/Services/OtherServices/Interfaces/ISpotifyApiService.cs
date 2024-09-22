
namespace Fafikv2.Services.OtherServices.Interfaces
{
    public interface ISpotifyApiService
    {
        public Task<string?[]> GetGenresOfTrack(string query);
        public Task<List<string>> GetRecommendationsBasedOnInput(string userInput);
        public Task<List<string>> GetRecommendationBasenOnGenre(string? userInput);
    }
}
