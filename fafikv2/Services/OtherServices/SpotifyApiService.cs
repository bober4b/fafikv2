using System.Text;
using System.Text.Json;
using Fafikv2.Services.OtherServices.Interfaces;

namespace Fafikv2.Services.OtherServices
{
    public class SpotifyApiService : ISpotifyApiService
    {
        private readonly string clientId= "47ac141b5aee4fd69e49c0688550a2f6";
        private readonly string clientSecret= "6c98228d6012463ab6ea4d91311f96d5";

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

            var response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseJson = JsonDocument.Parse(responseBody);
            return responseJson.RootElement.GetProperty("access_token").GetString();
        }

        private async Task<JsonDocument> SearchTrackAsync(string trackName, string artistName, string? accessToken)
        {
            var url = $"https://api.spotify.com/v1/search?q=track:{trackName}%20artist:{artistName}&type=track&limit=1";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonDocument.Parse(responseBody);
        }

        private async Task<JsonDocument> GetTrackDetailsAsync(string? trackId, string? accessToken)
        {
            var url = $"https://api.spotify.com/v1/tracks/{trackId}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonDocument.Parse(responseBody);
        }

        private async Task<JsonDocument> GetArtistDetailsAsync(string? artistId, string? accessToken)
        {
            var url = $"https://api.spotify.com/v1/artists/{artistId}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonDocument.Parse(responseBody);
        }

        public async Task<string?[]> GetGenresOfTrackAsync(string trackName, string artistName)
        {
            var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
            var searchResult = await SearchTrackAsync(trackName, artistName, accessToken).ConfigureAwait(false);

            var trackId = searchResult.RootElement
                .GetProperty("tracks")
                .GetProperty("items")[0]
                .GetProperty("id")
                .GetString();

            var trackDetails = await GetTrackDetailsAsync(trackId, accessToken).ConfigureAwait(false);
            var artistId = trackDetails.RootElement
                .GetProperty("artists")[0]
                .GetProperty("id")
                .GetString();

            var artistDetails = await GetArtistDetailsAsync(artistId, accessToken).ConfigureAwait(false);
            var genres = artistDetails.RootElement.GetProperty("genres").EnumerateArray();

            return genres.Select(genre => genre.GetString()).ToArray();
        }
    }
}
