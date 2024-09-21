using DSharpPlus.CommandsNext;
using Fafikv2.Data.Models;
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
    public async Task AddBannedWord(CommandContext ctx, string bannedWord, int time)
    {
        if (ctx.Member != null && ctx.Member.IsOwner)
        {
            var result=await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                var bannedWords = new BannedWords
                {
                    ServerConfig = await _serverConfigService.GetServerConfig(Guid.Parse($"{ctx.Guild.Id:X32}"))
                         ,
                    BannedWord = bannedWord,
                    Id = Guid.NewGuid(),
                    Time = time
                };

                var result=await _bannedWordsService
                    .Add(bannedWords)
                     ;
                return result;

            }) ;

            if(result)
                await ctx.RespondAsync("Słowo dodane do bazy.") ;
            else
            {
                await ctx.RespondAsync("słowo jest już zabronione!!!") ;
            }
        }
        else
        {
            await ctx.RespondAsync("Brak uprawnień!!!") ;
        }
    }
    public async Task DelBannedWord(CommandContext ctx, string delWord)
    {
        if (ctx.Member != null && ctx.Member.IsOwner)
        {
            var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () => await _bannedWordsService
                    .Remove(delWord, Guid.Parse($"{ctx.Guild.Id:X32}"))
                 )
                 ;

            if (result)
                await ctx.RespondAsync("Słowo zostało usunięte") ;
            else
            {
                await ctx.RespondAsync("Słowo nie jest zabronione!!!") ;
            }

        }
        else
        {
            await ctx.RespondAsync("Brak uprawnień!!!") ;
        }
    }
    public async Task BanEnable(CommandContext ctx)
    {
        if (ctx.Member != null && ctx.Member.IsOwner)
        {
           await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                await _serverConfigService.EnableBans(Guid.Parse($"{ctx.Guild.Id:X32}")) ;
            }) ;
            await ctx.RespondAsync("Bans Enabled") ;
        }
        else
        {
            await ctx.RespondAsync("Brak uprawnień!!!") ;
        }
    }
    public async Task BanDisable(CommandContext ctx)
    {
        if (ctx.Member != null && ctx.Member.IsOwner)
        {
            await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                await _serverConfigService.DisableBans(Guid.Parse($"{ctx.Guild.Id:X32}")) ;
            }) ;
            await ctx.RespondAsync("Bans Disabled") ;
        }
        else
        {
            await ctx.RespondAsync("Brak uprawnień!!!") ;
        }
    }
    public async Task KickEnable(CommandContext ctx)
    {
        if (ctx.Member != null && ctx.Member.IsOwner)
        {
            await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                await _serverConfigService.EnableKicks(Guid.Parse($"{ctx.Guild.Id:X32}")) ;
            }) ;
            await ctx.RespondAsync("Kicks Enabled") ;
        }
        else
        {
            await ctx.RespondAsync("Brak uprawnień!!!") ;
        }
    }
    public async Task KickDisable(CommandContext ctx)
    {
        if (ctx.Member != null && ctx.Member.IsOwner)
        {
            await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                await _serverConfigService.DisableKicks(Guid.Parse($"{ctx.Guild.Id:X32}")) ;
            }) ;
            await ctx.RespondAsync("Kicks Disabled") ;
        }
        else
        {
            await ctx.RespondAsync("Brak uprawnień!!!") ;
        }
    }
    public async Task AutoModeratorEnable(CommandContext ctx)
    {
        if (ctx.Member != null && ctx.Member.IsOwner)
        {
            await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                await _serverConfigService.EnableAutoModerator(Guid.Parse($"{ctx.Guild.Id:X32}")) ;
            }) ;
            await ctx.RespondAsync("AutoModerator Enabled") ;
        }
        else
        {
            await ctx.RespondAsync("Brak uprawnień!!!") ;
        }
    }
    public async Task AutoModeratorDisable(CommandContext ctx)
    {
        if (ctx.Member != null && ctx.Member.IsOwner)
        {
            await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                await _serverConfigService.DisableAutoModerator(Guid.Parse($"{ctx.Guild.Id:X32}")) ;
            }) ;
            await ctx.RespondAsync("AutoModerator Disabled") ;
        }
        else
        {
            await ctx.RespondAsync("Brak uprawnień!!!") ;
        }
    }
    public async Task AutoPlayEnabled(CommandContext ctx)
    {
        if (ctx.Member != null && ctx.Member.IsOwner)
        {
            await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                await _serverConfigService.EnableAutoPlay(Guid.Parse($"{ctx.Guild.Id:X32}")) ;
            }) ;
            await ctx.RespondAsync("AutoPlay Enabled") ;
        }
        else
        {
            await ctx.RespondAsync("Brak uprawnień!!!") ;
        }
    }
    public async Task AutoPlayDisabled(CommandContext ctx)
    {
        if (ctx.Member != null && ctx.Member.IsOwner)
        {
            await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                await _serverConfigService.DisableAutoPlay(Guid.Parse($"{ctx.Guild.Id:X32}")) ;
            }) ;
            await ctx.RespondAsync("AutoPlay Disabled") ;
        }
        else
        {
            await ctx.RespondAsync("Brak uprawnień!!!") ;
        }
    }
}