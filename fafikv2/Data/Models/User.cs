using System.ComponentModel.DataAnnotations;

namespace Fafikv2.Data.Models
{
    public class User
    {

        //DB info
        public virtual ICollection<ServerUsers>? Servers { get; set; }

        //DiscordData
        public Guid Id { get; set; }
        [MaxLength(32)]
        public string? Name { get; set; }
        

        //BotData karma

        public float GlobalKarma { get; set; }

        //BotData stats

        public int MessagesCountGlobal { get; set; }
        public int BotInteractionGlobal { get; set; }
        public int UserLevel { get; set; }


    }
}
