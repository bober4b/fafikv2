using System.ComponentModel.DataAnnotations;

namespace Fafikv2.Data.Models
{
    public class Song
    {
        public Guid Id { get; set; }
        [MaxLength(100)]
        public string? Title { get; set; }
        [MaxLength(100)]
        public string? Artist { get; set; }
        [MaxLength(200)]
        public string? Genres { get; set; }
        public Uri? LinkUri { get; set; }

    }
}
