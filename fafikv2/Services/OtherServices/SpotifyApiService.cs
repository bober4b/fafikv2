﻿using Fafikv2.Configuration.ConfigJSON;
using Fafikv2.Services.OtherServices.Interfaces;
using System.Text;
using System.Text.Json;
using Serilog;

namespace Fafikv2.Services.OtherServices
{
    public class SpotifyApiService : ISpotifyApiService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;

        public SpotifyApiService()
        {
            var jsonReader = new JsonReader();
            jsonReader.ReadJsonAsync().Wait();
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

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseJson = JsonDocument.Parse(responseBody);
            return responseJson.RootElement.GetProperty("access_token").GetString();
        }

        private static async Task<JsonDocument> SearchTrack(string query, string? accessToken)
        {
            var builder = new UriBuilder("https://api.spotify.com/v1/search")
            {
                Query = $"q={Uri.EscapeDataString(query)}&type=track&limit=1"
            };

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(builder.Uri);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseBody);
        }

        private static async Task<JsonDocument> GetTrackDetails(string? trackId, string? accessToken)
        {
            var url = $"https://api.spotify.com/v1/tracks/{trackId}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseBody);
        }

        private static async Task<JsonDocument> GetArtistDetails(string? artistId, string? accessToken)
        {
            var url = $"https://api.spotify.com/v1/artists/{artistId}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseBody);
        }

        private static async Task<JsonDocument> GetRecommendations(string seedTracks, string seedArtists, string seedGenres, string? accessToken)
        {
            var url = $"https://api.spotify.com/v1/recommendations?seed_tracks={seedTracks}&seed_artists={seedArtists}&seed_genres={seedGenres}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseBody);
        }

        public async Task<string?[]> GetGenresOfTrack(string query)
        {
            var accessToken = await GetAccessToken();
            JsonDocument searchResult;

            if (PolishLettersConverter.ContainsPolishChars(query)) query = PolishLettersConverter.ReplacePolishChars(query);

            if (Uri.TryCreate(query, UriKind.Absolute, out _))
            {
                var sort = query.Split('/');
                query = sort.Last();
            }

            try
            {
                searchResult = await SearchTrack(query, accessToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error searching track");
                return Array.Empty<string>();
            }


            if (searchResult.RootElement.GetProperty("tracks").GetProperty("items").GetArrayLength() == 0)
            {
                Log.Warning("No tracks found matching the query.");
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
                Log.Error(ex, "Error extracting track ID");
                return Array.Empty<string>();
            }

            JsonDocument trackDetails;
            try
            {
                trackDetails = await GetTrackDetails(trackId, accessToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting track details");
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
                Log.Error(ex, "Error extracting artist ID");
                return Array.Empty<string>();
            }

            JsonDocument artistDetails;
            try
            {
                artistDetails = await GetArtistDetails(artistId, accessToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting artist details");
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
                Log.Error(e, "Error extracting genres");
                throw;
            }
            return result;
        }

        public async Task<List<string>> GetRecommendationsBasedOnInput(string userInput)
        {
            string? accessToken;
            try
            {
                accessToken = await GetAccessToken();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error obtaining access token");
                return new List<string>();
            }

            if (userInput.ToLower().Contains("gatunek:"))
            {
                try
                {
                    var genre = userInput[(userInput.IndexOf(":", StringComparison.Ordinal) + 1)..].Trim();
                    var recommendations = await GetRecommendations(string.Empty, string.Empty, genre, accessToken);
                    return ExtractTrackDetails(recommendations);
                }
                catch (HttpRequestException httpEx)
                {
                    Log.Error(httpEx, "HTTP request error");
                    return new List<string>();
                }
                catch (JsonException jsonEx)
                {
                    Log.Error(jsonEx, "JSON parsing error");
                    return new List<string>();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Unexpected error");
                    return new List<string>();
                }
            }

            try
            {
                var searchResult = await SearchTrack(userInput, accessToken);

                if (searchResult.RootElement.GetProperty("tracks").GetProperty("items").GetArrayLength() == 0)
                {
                    return new List<string>();
                }

                var trackId = searchResult.RootElement
                    .GetProperty("tracks")
                    .GetProperty("items")[0]
                    .GetProperty("id")
                    .GetString();

                var trackDetails = await GetTrackDetails(trackId, accessToken);
                var artistId = trackDetails.RootElement
                    .GetProperty("artists")[0]
                    .GetProperty("id")
                    .GetString();

                var recommendations = await GetRecommendations(trackId!, artistId!, string.Empty, accessToken);
                return ExtractTrackDetails(recommendations);
            }
            catch (HttpRequestException httpEx)
            {
                Log.Error(httpEx, "HTTP request error");
                return new List<string>();
            }
            catch (JsonException jsonEx)
            {
                Log.Error(jsonEx, "JSON parsing error");
                return new List<string>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetRecommendationBasenOnGenre(string? userInput)
        {
            string? accessToken;
            try
            {
                accessToken = await GetAccessToken();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error obtaining access token");
                return new List<string>();
            }

            try
            {
                var genre = userInput?.Trim(' '); 
                var recommendations = await GetRecommendations(string.Empty, string.Empty, genre!, accessToken);
                return ExtractTrackDetails(recommendations);
            }
            catch (HttpRequestException httpEx)
            {
                Log.Error(httpEx, "HTTP request error");
                return new List<string>();
            }
            catch (JsonException jsonEx)
            {
                Log.Error(jsonEx, "JSON parsing error");
                return new List<string>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error");
                return new List<string>();
            }
        }

        private static List<string> ExtractTrackDetails(JsonDocument recommendations)
        {
            var tracks = recommendations.RootElement.GetProperty("tracks").EnumerateArray();

            return (from track in tracks.Take(10)
                    let title = track.GetProperty("name").GetString()
                    let artist = track.GetProperty("artists")[0].GetProperty("name").GetString()
                    select $"{artist} - {title}").ToList();

        }
    }
}
