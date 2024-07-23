namespace Fafikv2.Data.Models
{
    public class Song
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Genres { get; set; }
        public Uri LinkUri { get; set; }

    }
}
