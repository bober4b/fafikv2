using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fafikv2.Data.Models
{
    public class UserServerStats
    {
        public Guid Id { get; set; }

        //server stats
        public float ServerKarma { get; set; } 
        public int MessagesCountServer { get; set; } 
        public int BotInteractionServer { get; set; } 
    }
}
