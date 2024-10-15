
using System.ComponentModel.DataAnnotations;

namespace Fafikv2.Data.Models
{
    public class Server
    {
        public virtual ICollection<ServerUsers>? Users { get; set; }
        public Guid Id { get; set; }
        [MaxLength(100)]
        public string? Name { get; set; }

        public Guid? ConfigId { get; set; }
        public ServerConfig? Config { get; set; }

    }
}
