using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;
using Fafikv2.Services.dbServices.Interfaces;

namespace Fafikv2.Services.dbServices;

public class BannedWordsService : IBannedWordsService
{
    private readonly IBannedWordsRepository _bannedWordsRepository;

    public BannedWordsService(IBannedWordsRepository bannedWordsRepository)
    {
        _bannedWordsRepository= bannedWordsRepository;
    }
    public async Task Add(BannedWords bannedWords)
    {
        await _bannedWordsRepository.Add(bannedWords).ConfigureAwait(false);
    }

    public async Task Remove(BannedWords bannedWords, Server server)
    {
        await _bannedWordsRepository.Remove(bannedWords,server).ConfigureAwait(false);
    }

    public async Task<bool> IsBanned(string bannedWord, Guid server)
    {
        var res= _bannedWordsRepository.GetBannedWordsByServer(server).FirstOrDefault(x=>x.BannedWord==bannedWord);

        return res != null;
    }

    public async Task<IEnumerable<BannedWords>> GetAll(Guid server)
    {
       var result= _bannedWordsRepository.GetBannedWordsByServer(server);
       return result;
    }
}