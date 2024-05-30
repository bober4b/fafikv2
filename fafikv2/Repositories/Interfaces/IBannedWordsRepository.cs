using Fafikv2.Data.Models;
namespace Fafikv2.Repositories.Interfaces
{
    public interface IBannedWordsRepository
    {
        public Task<bool> Add(BannedWords bannedWords);
        public Task<bool> Remove(string bannedWords, Guid server);

        public IEnumerable<BannedWords> GetBannedWordsByServer(Guid server);
    }
}
