using DSharpPlus.CommandsNext;
using Fafikv2.Configuration.ConfigJSON;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Web;
using Fafikv2.Commands.MessageCreator;
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

        public AdditionalMusicService()
        {
            var jsonReader = new JsonReader();
            _ = jsonReader.ReadJson();
            _apiKey = jsonReader.GeniusToken;
        }
        public async Task FindLyric(CommandContext ctx, string title, string artist)
        {
            try
            {
                var songId = await GetSongId(_apiKey, title, artist);
                if (!string.IsNullOrEmpty(songId))
                {
                    var lyrics = await GetLyrics(_apiKey, songId);
                    var songUrl = $"https://genius.com/songs/{songId}";

                    // Jeśli tekst przekracza 4096 znaków, skracamy go
                    var truncatedLyrics = lyrics is { Length: > 4096 } ? lyrics[..4093] + "..." : lyrics;

                    // Tworzy embed z tekstem piosenki (maksymalnie 4096 znaków)
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"Lyrics for '{title}' by {artist}",
                        Description = truncatedLyrics,
                        Color = DiscordColor.Purple,
                        Url = songUrl
                    };

                    var messageBuilder = new DiscordMessageBuilder().AddEmbed(embed);

                    await ctx.RespondAsync(messageBuilder);
                }
                else
                {
                    // Gdy nie znaleziono piosenki
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


        private static async Task<string?> GetSongId(string apiKey, string songTitle, string artist)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var url = $"https://api.genius.com/search?q={Uri.EscapeDataString(songTitle + " " + artist)}";
            var response = await Client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Błąd HTTP: {response.StatusCode}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);
            var song = json["response"]?["hits"]?.First?["result"];

            return song?["id"]?.ToString();
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

            if (string.IsNullOrEmpty(lyricsPath)) return null;
            var lyricsUrl = "https://genius.com" + lyricsPath;
            return await GetLyricsFromPage(lyricsUrl);

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

            // Znajdź elementy zawierające tekst piosenki
            var lyricsNode = htmlDoc.DocumentNode.SelectNodes("//div[@data-lyrics-container]") ?? throw new Exception("Nie znaleziono tekstu piosenki na stronie.");
            // Wyciągnij tekst z wszystkich odpowiednich elementów i zachowaj formatowanie

            return lyricsNode.Aggregate("", (current, lyric) => current + string.Join("\n", lyric.DescendantsAndSelf()
                .Where(n => n.NodeType == HtmlNodeType.Text && !string.IsNullOrWhiteSpace(n.InnerText))
                .Select(n => HttpUtility.HtmlDecode(n.InnerText.Trim()))));

        }
    }
}
