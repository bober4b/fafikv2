
namespace Fafikv2.Data.Models
{
    public class ServerUsers
    {
        

        
        public User User { get; set; }
        public Guid UserId { get; set; }
        public Server Server { get; set; }
        public Guid ServerId { get; set; }

       public Guid? UserServerStatsId { get; set; }
       public UserServerStats UserServerStats { get; set; }


    }
}
