using System.Text;
using System.Text.Json;
using Fafikv2.Configuration.ConfigJSON;
using Fafikv2.Services.OtherServices.Interfaces;

namespace Fafikv2.Services.OtherServices
{
    public class SpotifyApiService : ISpotifyApiService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;

        public SpotifyApiService()
        {
            var jsonReader = new JsonReader();
            jsonReader.ReadJson().Wait();
            _clientId = jsonReader.SpotifyClientId;
            _clientSecret = jsonReader.SpotifyClientToken;
        }

        private async Task<string?> GetAccessTokenAsync()
        {
            const string url = "https://accounts.spotify.com/api/token";
            var client = new HttpClient();
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));

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

        private static async Task<JsonDocument> SearchTrackAsync(string query, string? accessToken)
        {
            var builder = new UriBuilder("https://api.spotify.com/v1/search");
            var queryParams = $"q={Uri.EscapeDataString(query)}&type=track&limit=1";
            builder.Query = queryParams;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(builder.Uri).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonDocument.Parse(responseBody);
        }

        private static async Task<JsonDocument> GetTrackDetailsAsync(string? trackId, string? accessToken)
        {
            var url = $"https://api.spotify.com/v1/tracks/{trackId}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonDocument.Parse(responseBody);
        }

        private static async Task<JsonDocument> GetArtistDetailsAsync(string? artistId, string? accessToken)
        {
            var url = $"https://api.spotify.com/v1/artists/{artistId}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonDocument.Parse(responseBody);
        }
        private async Task<JsonDocument> GetRecommendationsAsync(string seedTracks, string seedArtists, string seedGenres, string accessToken)
        {
            var url = $"https://api.spotify.com/v1/recommendations?seed_tracks={seedTracks}&seed_artists={seedArtists}&seed_genres={seedGenres}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseBody);
        }

        public async Task<string?[]> GetGenresOfTrackAsync(string query)
        {
            var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
            JsonDocument searchResult;
            try
            {
                searchResult = await SearchTrackAsync(query, accessToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching track: {ex.Message}");
                return Array.Empty<string>();
            }

            if (searchResult.RootElement.GetProperty("tracks").GetProperty("items").GetArrayLength() == 0)
            {
                Console.WriteLine("No tracks found matching the query.");
                return Array.Empty<string>();
            }

            string trackId;
            try
            {
                trackId = searchResult.RootElement
                    .GetProperty("tracks")
                    .GetProperty("items")[0]
                    .GetProperty("id")
                    .GetString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting track ID: {ex.Message}");
                return Array.Empty<string>();
            }

            JsonDocument trackDetails;
            try
            {
                trackDetails = await GetTrackDetailsAsync(trackId, accessToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting track details: {ex.Message}");
                return Array.Empty<string>();
            }

            string artistId;
            try
            {
                artistId = trackDetails.RootElement
                    .GetProperty("artists")[0]
                    .GetProperty("id")
                    .GetString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting artist ID: {ex.Message}");
                return Array.Empty<string>();
            }

            JsonDocument artistDetails;
            try
            {
                artistDetails = await GetArtistDetailsAsync(artistId, accessToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting artist details: {ex.Message}");
                return Array.Empty<string>();
            }

            var genres = artistDetails.RootElement.GetProperty("genres").EnumerateArray();

            return genres.Select(genre => genre.GetString()).ToArray();
        }

        public async Task<string> GetRecommendationsBasedOnInputAsync(string userInput)
        {
            var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);

            // Zakładamy, że jeśli input zawiera słowo "gatunek: ", to jest to gatunek, w przeciwnym razie traktujemy to jako utwór
            if (userInput.ToLower().Contains("gatunek:"))
            {
                var genre = userInput[(userInput.IndexOf(":", StringComparison.Ordinal) + 1)..].Trim();
                var recommendations = await GetRecommendationsAsync(string.Empty, string.Empty, genre, accessToken).ConfigureAwait(false);
                return ExtractTrackDetails(recommendations);
            }
            else
            {
                var searchResult = await SearchTrackAsync(userInput, accessToken).ConfigureAwait(false);

                if (searchResult.RootElement.GetProperty("tracks").GetProperty("items").GetArrayLength() == 0)
                {
                    throw new Exception("Nie znaleziono utworów pasujących do zapytania.");
                }

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

                var recommendations = await GetRecommendationsAsync(trackId, artistId, string.Empty, accessToken).ConfigureAwait(false);
                return ExtractTrackDetails(recommendations);
            }
        }

        private static string ExtractTrackDetails(JsonDocument recommendations)
        {
            var tracks = recommendations.RootElement.GetProperty("tracks").EnumerateArray();
            var trackDetailsList = tracks.Select(track =>
            {
                var title = track.GetProperty("name").GetString();
                var artist = track.GetProperty("artists")[0].GetProperty("name").GetString();
                return $"{artist} - {title}";
            }).ToList();

            return string.Join(", ", trackDetailsList);
        }
    }
}
