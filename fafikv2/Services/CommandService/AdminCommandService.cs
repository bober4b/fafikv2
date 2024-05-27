using DSharpPlus.CommandsNext;
using Fafikv2.Data.Models;
using Fafikv2.Services.dbServices;
using Fafikv2.Services.dbServices.Interfaces;
using Fafikv2.Services.OtherServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Fafikv2.Services.CommandService;

public class AdminCommandService
{
    private readonly IBannedWordsService _bannedWordsService;
    private readonly IDatabaseContextQueueService _databaseContextQueueService;
    private readonly IServerConfigService _serverConfigService;

    public AdminCommandService(ServiceProvider serviceProvider)
    {
        _bannedWordsService = serviceProvider.GetRequiredService<IBannedWordsService>();
        _databaseContextQueueService = serviceProvider.GetRequiredService<IDatabaseContextQueueService>();
        _serverConfigService = serviceProvider.GetRequiredService<IServerConfigService>();
    }

    public async Task addBannedWord(CommandContext ctx, string bannedWord)
    {
        if (ctx.Member.IsOwner)
        {
            await _databaseContextQueueService.EnequeDatabaseTask(async () =>
            {
                BannedWords bannedWords = new BannedWords
                {
                    ServerConfig = await _serverConfigService.GetServerConfig(Guid.Parse($"{ctx.Guild.Id:X32}"))
                        .ConfigureAwait(false),
                    BannedWord = bannedWord,
                    Id = Guid.NewGuid()
                };
                _bannedWordsService.Add(bannedWords);
            }).ConfigureAwait(false);

            
            await ctx.RespondAsync("benc").ConfigureAwait(false);
        }
    }
}