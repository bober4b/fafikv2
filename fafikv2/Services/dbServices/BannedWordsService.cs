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
    public async Task<bool> Add(BannedWords bannedWords)
    {
        
        return await _bannedWordsRepository.Add(bannedWords) ;
         
    }

    public async Task<bool> Remove(string bannedWords, Guid server)
    {
        return await _bannedWordsRepository.Remove(bannedWords,server) ;
        
    }

    public Task<bool> IsBanned(string bannedWord, Guid server)
    {
        var res= _bannedWordsRepository.GetBannedWordsByServer(server).FirstOrDefault(x=>x.BannedWord==bannedWord);
        
        return Task.FromResult(res != null);
        
    }

    public Task<IEnumerable<BannedWords>> GetAllByServer(Guid server)
    {
       var result= _bannedWordsRepository.GetBannedWordsByServer(server);
       return Task.FromResult(result);
    }
}