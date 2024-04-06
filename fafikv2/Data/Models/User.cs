namespace Fafikv2.Data.Models
{
    public class User
    {

        //DB info
        public virtual ICollection<ServerUsers> Servers { get; set; }

        //DiscordData
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }

        //BotData karma

        public float GlobalKarma { get; set; }

        //BotData stats

        public int MessagesCountGlobal { get; set; }
        public int BotInteractionGlobal { get; set; }
        public int UserLevel { get; set; }


    }
}
