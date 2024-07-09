using Fafikv2.Data.Models;

namespace Fafikv2.Services.OtherServices
{
    public class CheckMessagesResult
    { 
        public bool Result { get; set; }
        public IEnumerable<BannedWords> Words { get; set; }
    }
}
