
namespace Fafikv2.Data.Models
{
    public class ServerConfig
    {
        public Guid Id { get; set; }
        public Guid? ServerId { get; set; }
        public virtual Server? Server { get; set; }
        public virtual ICollection<BannedWords> BannedWords { get; set; }

        public bool BansEnabled { get; set; }

        public bool AutoModeratorEnabled { get; set; }
    }
}
