using System.Text;
using System.Text.Json;
using Fafikv2.Services.OtherServices.Interfaces;

namespace Fafikv2.Services.OtherServices
{
    public class SpotifyApiService : ISpotifyApiService
    {
        private readonly string clientId= "";
        private readonly string clientSecret= "";

        private async Task<string?> GetAccessTokenAsync()
        {
            var url = "https://accounts.spotify.com/api/token";
            var client = new HttpClient();
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Headers =
                {
                    Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader)
                },
                Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded")
            };

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseJson = JsonDocument.Parse(responseBody);
            return responseJson.RootElement.GetProperty("access_token").GetString();
        }

        private async Task<JsonDocument> SearchTrackAsync(string query, string? accessToken)
        {
            var url = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=track&limit=1";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseBody);
        }

        private async Task<JsonDocument> GetTrackDetailsAsync(string? trackId, string? accessToken)
        {

            var url = $"https://api.spotify.com/v1/tracks/{trackId}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseBody);
        }

        private async Task<JsonDocument> GetArtistDetailsAsync(string? artistId, string? accessToken)
        {

            var url = $"https://api.spotify.com/v1/artists/{artistId}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseBody);
        }

        public async Task<string?[]> GetGenresOfTrackAsync(string query)
        {
            var accessToken = await GetAccessTokenAsync();
            var searchResult = await SearchTrackAsync(query.Remove(0,5), accessToken);

            if (searchResult.RootElement.GetProperty("tracks").GetProperty("items").GetArrayLength() == 0)
            {
                throw new Exception("No tracks found matching the query.");
            }

            var trackId = searchResult.RootElement
                .GetProperty("tracks")
                .GetProperty("items")[0]
                .GetProperty("id")
                .GetString();

            var trackDetails = await GetTrackDetailsAsync(trackId, accessToken);
            var artistId = trackDetails.RootElement
                .GetProperty("artists")[0]
                .GetProperty("id")
                .GetString();

            var artistDetails = await GetArtistDetailsAsync(artistId, accessToken);
            var genres = artistDetails.RootElement.GetProperty("genres").EnumerateArray();
            var genreList = new List<string>();

            foreach (var genre in genres)
            {
                genreList.Add(genre.GetString());
            }

            return genreList.ToArray();
        }
    }
}
