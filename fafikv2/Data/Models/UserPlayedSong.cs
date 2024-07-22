namespace Fafikv2.Data.Models
{
    public class UserPlayedSong
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
        public Guid SongId { get; set; }
        public virtual Song Song { get; set; }


    }
}
