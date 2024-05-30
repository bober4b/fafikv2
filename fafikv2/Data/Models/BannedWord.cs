namespace Fafikv2.Data.Models
{
    public class BannedWords
    {
        public Guid Id { get; set; }
        public Guid ServerConfigId { get; set; }
        public string BannedWord { get; set; }
        public virtual ServerConfig ServerConfig { get; set; }
    }
}
