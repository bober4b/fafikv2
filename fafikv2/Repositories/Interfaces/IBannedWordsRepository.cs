using Fafikv2.Data.Models;
namespace Fafikv2.Repositories.Interfaces
{
    public interface IBannedWordsRepository
    {
        public Task Add(BannedWords bannedWords);
        public Task Remove(BannedWords bannedWords, Server server);

        public IEnumerable<BannedWords> GetBannedWordsByServer(Guid server);
    }
}
