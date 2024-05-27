using Fafikv2.Data.Models;
namespace Fafikv2.Services.dbServices.Interfaces;

public interface IBannedWordsService
{
    public Task Add(BannedWords  bannedWords);
    public Task Remove(BannedWords bannedWords, Server server);

    public Task<bool> IsBanned(string bannedWords, Guid server);

    public Task<IEnumerable<BannedWords>> GetAll(Guid server);

}