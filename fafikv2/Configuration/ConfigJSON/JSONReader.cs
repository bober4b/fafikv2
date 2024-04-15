using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fafikv2.Configuration.ConfigJSON
{
    internal class JsonReader
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public async Task ReadJson()
        {
            using (StreamReader sr = new StreamReader("config.json"))
            {
                string json = await sr.ReadToEndAsync();
                JsonStructure data = JsonConvert.DeserializeObject<JsonStructure>(json);

                Token = data.Token;
                Prefix = data.Prefix;
            }
        }
    }

    internal sealed class JsonStructure
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
    }

}
