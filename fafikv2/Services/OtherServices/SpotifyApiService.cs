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
        private readonly PolishLettersConverter _converter=new();

        public SpotifyApiService()
        {
            var jsonReader = new JsonReader();
            jsonReader.ReadJson().Wait();
            _clientId = jsonReader.SpotifyClientId;
            _clientSecret = jsonReader.SpotifyClientToken;
        }

        private async Task<string?> GetAccessToken()
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

        private static async Task<JsonDocument> SearchTrack(string query, string? accessToken)
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

        private static async Task<JsonDocument> GetTrackDetails(string? trackId, string? accessToken)
        {
            var url = $"https://api.spotify.com/v1/tracks/{trackId}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonDocument.Parse(responseBody);
        }

        private static async Task<JsonDocument> GetArtistDetails(string? artistId, string? accessToken)
        {
            var url = $"https://api.spotify.com/v1/artists/{artistId}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonDocument.Parse(responseBody);
        }

        private static async Task<JsonDocument> GetRecommendations(string seedTracks, string seedArtists, string seedGenres, string? accessToken)
        {
            var url = $"https://api.spotify.com/v1/recommendations?seed_tracks={seedTracks}&seed_artists={seedArtists}&seed_genres={seedGenres}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonDocument.Parse(responseBody);
        }

        public async Task<string?[]> GetGenresOfTrack(string query)
        {
            var accessToken = await GetAccessToken().ConfigureAwait(false);
            JsonDocument searchResult;

            if (_converter.ContainsPolishChars(query)) query=_converter.ReplacePolishChars(query);

            if ( Uri.TryCreate(query, UriKind.Absolute, out _))
            {
                // If search is a valid URL, use the URI overload
                var sort = query.Split('/');
                query = sort.Last();
            }

            try
            {
                searchResult = await SearchTrack(query, accessToken).ConfigureAwait(false);
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

            string? trackId;
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
                trackDetails = await GetTrackDetails(trackId, accessToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting track details: {ex.Message}");
                return Array.Empty<string>();
            }

            string? artistId;
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
                artistDetails = await GetArtistDetails(artistId, accessToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting artist details: {ex.Message}");
                return Array.Empty<string>();
            }

            var genres = artistDetails.RootElement.GetProperty("genres").EnumerateArray();

            string?[] result;
            try
            {
                result = genres.Select(genre => genre.GetString()).ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            return result;
        }

        public async Task<string> GetRecommendationsBasedOnInput(string userInput)
        {
            string? accessToken;
            try
            {
                accessToken = await GetAccessToken().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obtaining access token: {ex.Message}");
                return "Error obtaining access token.";
            }

            if (userInput.ToLower().Contains("gatunek:"))
            {
                try
                {
                    var genre = userInput[(userInput.IndexOf(":", StringComparison.Ordinal) + 1)..].Trim();
                    var recommendations = await GetRecommendations(string.Empty, string.Empty, genre, accessToken).ConfigureAwait(false);
                    return ExtractTrackDetails(recommendations);
                }
                catch (HttpRequestException httpEx)
                {
                    Console.WriteLine($"HTTP request error: {httpEx.Message}");
                    return "Błąd podczas wykonywania żądania HTTP.";
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"JSON parsing error: {jsonEx.Message}");
                    return "Błąd podczas przetwarzania danych JSON.";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    return "Wystąpił nieoczekiwany błąd.";
                }
            }

            try
            {
                var searchResult = await SearchTrack(userInput, accessToken).ConfigureAwait(false);

                if (searchResult.RootElement.GetProperty("tracks").GetProperty("items").GetArrayLength() == 0)
                {
                    return "Nie znaleziono utworów pasujących do zapytania.";
                }

                var trackId = searchResult.RootElement
                    .GetProperty("tracks")
                    .GetProperty("items")[0]
                    .GetProperty("id")
                    .GetString();

                var trackDetails = await GetTrackDetails(trackId, accessToken).ConfigureAwait(false);
                var artistId = trackDetails.RootElement
                    .GetProperty("artists")[0]
                    .GetProperty("id")
                    .GetString();

                var recommendations = await GetRecommendations(trackId!, artistId!, string.Empty, accessToken).ConfigureAwait(false);
                return ExtractTrackDetails(recommendations);
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP request error: {httpEx.Message}");
                return "Błąd podczas wykonywania żądania HTTP.";
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON parsing error: {jsonEx.Message}");
                return "Błąd podczas przetwarzania danych JSON.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return "Wystąpił nieoczekiwany błąd.";
            }
        }

        public async Task<string> GetRecommendationBasenOnGenre(string? userInput)
        {
            string? accessToken;
            try
            {
                accessToken = await GetAccessToken().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obtaining access token: {ex.Message}");
                return "Error obtaining access token.";
            }

            try
            {
                var genre = userInput.Trim(' '); // Użycie całego inputu jako nazwy gatunku
                var recommendations = await GetRecommendations(string.Empty, string.Empty, genre, accessToken).ConfigureAwait(false);
                return ExtractTrackDetails(recommendations);
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP request error: {httpEx.Message}");
                return "Błąd podczas wykonywania żądania HTTP.";
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON parsing error: {jsonEx.Message}");
                return "Błąd podczas przetwarzania danych JSON.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return "Wystąpił nieoczekiwany błąd.";
            }
        }

        private static string ExtractTrackDetails(JsonDocument recommendations)
        {
            var tracks = recommendations.RootElement.GetProperty("tracks").EnumerateArray();
            var firstTrack = tracks.FirstOrDefault(); // Pobierz pierwszy element lub null, jeśli lista jest pusta

            if (firstTrack.ValueKind == JsonValueKind.Undefined) return string.Empty;

            var title = firstTrack.GetProperty("name").GetString();
            var artist = firstTrack.GetProperty("artists")[0].GetProperty("name").GetString();
            return $"{artist} - {title}";

        }
    }
}
