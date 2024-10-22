using Newtonsoft.Json;

namespace Fafikv2.Configuration.ConfigJSON
{
    public class JsonReader
    {
        public string Token { get; set; } = null!;
        public string Prefix { get; set; } = null!;
        public string GeniusToken { get; set; } = null!;
        public string SpotifyClientId { get; set; } = null!;
        public string SpotifyClientToken { get; set; } = null!;

        public async Task ReadJson()
        {
            using var sr = new StreamReader("config.json");
            var json = await sr.ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<JsonStructure>(json);

            if (data != null)
            {
                Token = data.Token;
                Prefix = data.Prefix;
                GeniusToken = data.GeniusToken;
                SpotifyClientId = data.SpotifyClientId;
                SpotifyClientToken = data.SpotifyClientToken;
            }
        }
    }
}
