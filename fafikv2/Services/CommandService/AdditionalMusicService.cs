using System.Net.Http.Headers;
using System.Web;
using DSharpPlus.CommandsNext;
using Fafikv2.Configuration.ConfigJSON;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace Fafikv2.Services.CommandService
{
    public class AdditionalMusicService
    {
        private static readonly HttpClient Client = new(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate })
        {
            Timeout = TimeSpan.FromSeconds(10) // Ustawienie limitu czasowego
        };

        private readonly string _apiKey;

        public AdditionalMusicService(JsonReader reader)
        {
            _apiKey = reader.GeniusToken;
        }

        public async Task FindLyric(CommandContext ctx, string title, string artist)
        {
            try
            {
                var songId = await GetSongId(_apiKey, title, artist).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(songId))
                {
                    var lyrics = await GetLyrics(_apiKey, songId).ConfigureAwait(false);
                    if (lyrics is { Length: >= 2000 })
                    {
                        var chunks = Enumerable.Range(0, (int)Math.Ceiling((double)lyrics.Length / 2000))
                            .Select(i => lyrics.Substring(i * 2000, Math.Min(2000, lyrics.Length - i * 2000)));
                        foreach (var partial in chunks )
                        {
                            await ctx.RespondAsync(partial).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        if (lyrics != null) await ctx.RespondAsync(lyrics).ConfigureAwait(false);
                    }
                }
                else
                {
                    await ctx.RespondAsync("Nie znaleziono piosenki.").ConfigureAwait(false);
                    Console.WriteLine("Nie znaleziono piosenki.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd: {ex.Message}");
            }
        }

        private static async Task<string?> GetSongId(string apiKey, string songTitle, string artist)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var url = $"https://api.genius.com/search?q={Uri.EscapeDataString(songTitle + " " + artist)}";
            var response = await Client.GetAsync(url).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Błąd HTTP: {response.StatusCode}");
            }

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var json = JObject.Parse(responseBody);
            var song = json["response"]?["hits"]?.First?["result"];

            return song?["id"]?.ToString();
        }

        private static async Task<string?> GetLyrics(string apiKey, string? songId)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var url = $"https://api.genius.com/songs/{songId}";
            var response = await Client.GetAsync(url).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Błąd HTTP: {response.StatusCode}");
            }

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var json = JObject.Parse(responseBody);
            var lyricsPath = json["response"]?["song"]?["path"]?.ToString();

            if (string.IsNullOrEmpty(lyricsPath)) return null;
            var lyricsUrl = "https://genius.com" + lyricsPath;
            return await GetLyricsFromPage(lyricsUrl).ConfigureAwait(false);

        }

        private static async Task<string?> GetLyricsFromPage(string url)
        {
            var response = await Client.GetAsync(url).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Błąd HTTP: {response.StatusCode}");
            }

            var pageContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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
