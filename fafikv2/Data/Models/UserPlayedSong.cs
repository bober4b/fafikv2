namespace Fafikv2.Data.Models
{
    public class UserPlayedSong
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public Guid SongId { get; set; }
        public Song? Song { get; set; }


    }
}
