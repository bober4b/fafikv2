using System.ComponentModel.DataAnnotations;

namespace Fafikv2.Data.Models
{
    public class UserServerStats
    {
        public Guid Id { get; set; }

        public Guid? ServerUserId { get; set; }
        public virtual ServerUsers? ServerUsers { get; set; }

        //server stats
        public float ServerKarma { get; set; }
        public int MessagesCountServer { get; set; }
        public int BotInteractionServer { get; set; }

        //server info
        [MaxLength(32)]
        public string? DisplayName { get; set; }

        //server strikes
        public int Penalties { get; set; }
        public DateTime ? LastPenaltyDate { get; set;}

    }
}
