using Fafikv2.Repositories.Interfaces;
using Fafikv2.Data.Models;
using Fafikv2.Data.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Fafikv2.Repositories;

public class BannedWordsRepository : IBannedWordsRepository
{
    private readonly DiscordBotDbContext _context;

    public BannedWordsRepository(DiscordBotDbContext context)
    {
        _context = context;
    }
    public async Task<bool> Add(BannedWords bannedWords)
    {
        var result= _context.BannedWords
            .Any(x => x.ServerConfigId == bannedWords.ServerConfig.Id && x.BannedWord == bannedWords.BannedWord);

        if (result) return false;


        _context.BannedWords.Add(bannedWords);
        await _context.SaveChangesAsync() ;
        return true;


    }

    public async Task<bool> Remove(string bannedWords, Guid server)
    {
        var del = await _context.ServerConfigs
            .Where(x => x.ServerId == server)
            .SelectMany(sc => sc.BannedWords)
            .FirstOrDefaultAsync(bw => bw.BannedWord == bannedWords)
             ;
        if (del == null) return false;
        _context.BannedWords.Remove(del);
        await _context.SaveChangesAsync() ;
        return true;

    }

    public IEnumerable<BannedWords> GetBannedWordsByServer(Guid server)
    {
        var result = _context.ServerConfigs
            .Where(x => x.ServerId == server)
            .SelectMany(x => x.BannedWords);
        return result.AsEnumerable();
    }
}