using System.Collections.Specialized;
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

    public async Task AddBannedWord(CommandContext ctx, string bannedWord)
    {
        if (ctx.Member.IsOwner)
        {
            var result=await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                BannedWords bannedWords = new BannedWords
                {
                    ServerConfig = await _serverConfigService.GetServerConfig(Guid.Parse($"{ctx.Guild.Id:X32}"))
                        .ConfigureAwait(false),
                    BannedWord = bannedWord,
                    Id = Guid.NewGuid()
                };

                var result=await _bannedWordsService
                    .Add(bannedWords)
                    .ConfigureAwait(false);
                return result;

            }).ConfigureAwait(false);

            if(result)
                await ctx.RespondAsync("Słowo dodane do bazy.").ConfigureAwait(false);
            else
            {
                await ctx.RespondAsync("słowo jest już zabronione!!!").ConfigureAwait(false);
            }
        }
        else
        {
            await ctx.RespondAsync("Brak uprawnień!!!").ConfigureAwait(false);
        }
    }

    public async Task DelBannedWord(CommandContext ctx, String delWord)
    {
        if (ctx.Member.IsOwner)
        {
            var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () => await _bannedWordsService
                    .Remove(delWord, Guid.Parse($"{ctx.Guild.Id:X32}"))
                .ConfigureAwait(false))
                .ConfigureAwait(false);

            if (result)
                await ctx.RespondAsync("Słowo zostało usunięte").ConfigureAwait(false);
            else
            {
                await ctx.RespondAsync("Słowo nie jest zabronione!!!").ConfigureAwait(false);
            }

        }
        else
        {
            await ctx.RespondAsync("Brak uprawnień!!!").ConfigureAwait(false);
        }
    }
}