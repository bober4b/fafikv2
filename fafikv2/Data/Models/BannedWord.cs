using System.ComponentModel.DataAnnotations;

namespace Fafikv2.Data.Models
{
    public class BannedWords
    {
        public Guid Id { get; set; }
        public Guid ServerConfigId { get; set; }
        public virtual ServerConfig? ServerConfig { get; set; }

        [MaxLength(50)]
        public string? BannedWord { get; set; }
        public int Time { get; set; }

    }
}
