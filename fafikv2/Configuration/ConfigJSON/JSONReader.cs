using Newtonsoft.Json;

namespace Fafikv2.Configuration.ConfigJSON
{
    public class JsonReader
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public string GeniusToken { get; set; }
        public async Task ReadJson()
        {
            using (StreamReader sr = new StreamReader("config.json"))
            {
                var json = await sr.ReadToEndAsync().ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<JsonStructure>(json);

                Token = data.Token;
                Prefix = data.Prefix;
                GeniusToken = data.GeniusToken;
            }
        }
    }

    public sealed class JsonStructure
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public string GeniusToken { get; set; }
    }

}
