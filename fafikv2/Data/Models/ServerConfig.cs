
namespace Fafikv2.Data.Models
{
    public class ServerConfig
    {
        public Guid Id { get; set; }
        public Guid? ServerId { get; set; }
        public virtual Server? Server { get; set; }
        public virtual ICollection<BannedWords> BannedWords { get; set; }

        public bool BansEnabled { get; set; } = false;
        public bool KicksEnabled { get; set; } = false;
        public bool AutoModeratorEnabled { get; set; } = false;
        public bool AutoplayEnabled { get; set; } = false;

    }
}
