
namespace Fafikv2.Data.Models
{
    public class Server
    {
        public virtual ICollection<ServerUsers> Users { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }

        public Guid ConfigId { get; set; }
        public ServerConfig Config { get; set; }

    }
}
