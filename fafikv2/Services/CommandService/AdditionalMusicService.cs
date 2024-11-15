using DSharpPlus.CommandsNext;
using Fafikv2.Configuration.ConfigJSON;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Web;
using DSharpPlus.Entities;

namespace Fafikv2.Services.CommandService
{
    public class AdditionalMusicService
    {
        private static readonly HttpClient Client = new(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate })
        {
            Timeout = TimeSpan.FromSeconds(10) // Ustawienie limitu czasowego
        };

        private readonly string _apiKey;
        private readonly Dictionary<string, string> _songIdCache = new();

        public AdditionalMusicService()
        {
            var jsonReader = new JsonReader();
            _ = jsonReader.ReadJsonAsync();
            _apiKey = jsonReader.GeniusToken;

            if (!string.IsNullOrEmpty(_apiKey))
            {
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            }
            else
            {
                Console.WriteLine("API key for Genius is missing!");
            }
        }
        public async Task FindLyric(CommandContext ctx, string title, string artist)
        {
            try
            {
                var songId = await GetSongId(title, artist);
                if (!string.IsNullOrEmpty(songId))
                {
                    var lyrics = await GetLyrics(_apiKey, songId);
                    var songUrl = $"https://genius.com/songs/{songId}";

                    if (lyrics != null)
                    {
                        var lyricEmbeds = CreateLyricEmbeds(title, artist, lyrics, songUrl);
                        var messageBuilder = new DiscordMessageBuilder();
                        foreach (var embed in lyricEmbeds)
                        {
                            messageBuilder.AddEmbed(embed);
                        }

                        await ctx.RespondAsync(messageBuilder);
                    }
                    else
                    {
                        var notFoundEmbed = new DiscordEmbedBuilder
                        {
                            Title = "Lyrics Not Found",
                            Description = $"Could not find lyrics for '{title}' by {artist}.",
                            Color = DiscordColor.Red
                        };
                        await ctx.RespondAsync(embed: notFoundEmbed.Build());
                    }
                }
                else
                {
                    var notFoundEmbed = new DiscordEmbedBuilder
                    {
                        Title = "Lyric Not Found",
                        Description = $"Could not find lyrics for '{title}' by {artist}.",
                        Color = DiscordColor.Red
                    };
                    await ctx.RespondAsync(embed: notFoundEmbed.Build());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private async Task<string?> GetSongId( string songTitle, string artist)
        {
            var cacheKey = songTitle.ToLower() + artist.ToLower();
            if (_songIdCache.TryGetValue(cacheKey, out var cachedSongId))
            {
                return cachedSongId;
            }

            var url = $"https://api.genius.com/search?q={Uri.EscapeDataString(songTitle + " " + artist)}";
            var response = await Client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Błąd HTTP: {response.StatusCode}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);
            var song = json["response"]?["hits"]?.First?["result"];
            var songId = song?["id"]?.ToString();

            if (!string.IsNullOrEmpty(songId))
            {
                _songIdCache[cacheKey] = songId;
            }

            return songId;
        }

        private static async Task<string?> GetLyrics(string apiKey, string? songId)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var url = $"https://api.genius.com/songs/{songId}";
            var response = await Client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Błąd HTTP: {response.StatusCode}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);
            var lyricsPath = json["response"]?["song"]?["path"]?.ToString();



            return !string.IsNullOrEmpty(lyricsPath) ? await GetLyricsFromPage("https://genius.com" + lyricsPath) : null;

        }

        private static async Task<string?> GetLyricsFromPage(string url)
        {
            var response = await Client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Błąd HTTP: {response.StatusCode}");
            }

            var pageContent = await response.Content.ReadAsStringAsync();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageContent);

            var lyricsNode = htmlDoc.DocumentNode.SelectNodes("//div[@data-lyrics-container]");
            return lyricsNode == null
                ? throw new Exception("Nie znaleziono tekstu piosenki na stronie.")
                : lyricsNode.Aggregate("", (current, lyric) => current + string.Join("\n", lyric.DescendantsAndSelf()
                .Where(n => n.NodeType == HtmlNodeType.Text && !string.IsNullOrWhiteSpace(n.InnerText))
                .Select(n => HttpUtility.HtmlDecode(n.InnerText.Trim()))));
        }
        private static List<DiscordEmbedBuilder> CreateLyricEmbeds(string title, string artist, string lyrics, string songUrl)
        {
            var lyricEmbeds = new List<DiscordEmbedBuilder>();

            for (var i = 0; i < lyrics.Length; i += 4096)
            {
                var part = lyrics.Substring(i, Math.Min(4096, lyrics.Length - i));
                var embed = new DiscordEmbedBuilder
                {
                    Title = i == 0 ? $"Lyrics for '{title}' by {artist}" : null,
                    Description = part,
                    Color = DiscordColor.Purple,
                    Url = i == 0 ? songUrl : null
                };

                lyricEmbeds.Add(embed);
            }

            return lyricEmbeds;
        }
    }
}
