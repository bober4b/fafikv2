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
    public async Task Add(BannedWords bannedWords)
    {
        _context.BannedWords.Add(bannedWords);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Remove(BannedWords bannedWords, Server server)
    {
        var del = await _context.ServerConfigs
            .Where(x => x.ServerId == server.Id)
            .SelectMany(sc => sc.BannedWords)
            .FirstOrDefaultAsync(bw => bw.BannedWord == bannedWords.BannedWord)
            .ConfigureAwait(false);
        if (del != null)
        {
            _context.BannedWords.Remove(del);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public IEnumerable<BannedWords> GetBannedWordsByServer(Guid server)
    {
        var result = _context.ServerConfigs
            .Where(x => x.ServerId == server)
            .SelectMany(x => x.BannedWords);
        return result.AsEnumerable();
    }
}