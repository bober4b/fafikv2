namespace Fafikv2.Data.Models
{
    public class UserServerStats
    {
        public Guid Id { get; set; }

        //server stats
        public float ServerKarma { get; set; } 
        public int MessagesCountServer { get; set; } 
        public int BotInteractionServer { get; set; }

        //server info
        public string DisplayName { get; set; }
    }
}
