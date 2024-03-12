namespace Fafikv2.Data.Models
{
    public class User
    {
        //DiscordData
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }

        //BotData

        public float ServerKarma { get; set; }
        public float AllKarma { get; set; }



    }
}
