using Fafikv2.Data.Models;
namespace Fafikv2.Services.dbServices.Interfaces;

public interface IBannedWordsService
{
    public Task<bool> Add(BannedWords  bannedWords);
    public Task<bool> Remove(string bannedWords, Guid server);

    public Task<bool> IsBanned(string bannedWords, Guid server);

    public Task<IEnumerable<BannedWords>> GetAll(Guid server);

}