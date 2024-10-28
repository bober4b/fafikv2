using DSharpPlus.CommandsNext;
using Fafikv2.Data.Models;
using Fafikv2.Services.dbServices.Interfaces;
using Fafikv2.Services.OtherServices.Interfaces;
using Fafikv2.Data.DifferentClasses;
namespace Fafikv2.Services.CommandService;

public class AdminCommandService
{
    private readonly IBannedWordsService _bannedWordsService;
    private readonly IDatabaseContextQueueService _databaseContextQueueService;
    private readonly IServerConfigService _serverConfigService;

    public AdminCommandService(IBannedWordsService bannedWordsService, IDatabaseContextQueueService databaseContextQueueService, IServerConfigService serverConfigService)
    {

        _bannedWordsService = bannedWordsService;
        _databaseContextQueueService = databaseContextQueueService;
        _serverConfigService = serverConfigService;
    }

    private static bool HasPermission(CommandContext ctx) =>
        ctx.Member != null && ctx.Member.IsOwner;

    private static async Task RespondWithPermissionError(CommandContext ctx) =>
        await ctx.RespondAsync("Permission denied!!!");
    private async Task ExecuteDatabaseTaskWithResponse(CommandContext ctx, Func<Task<bool>> databaseTask, string successMessage, string failureMessage)
    {
        var result = await _databaseContextQueueService.EnqueueDatabaseTask(databaseTask);
        await ctx.RespondAsync(result ? successMessage : failureMessage);
    }

    public async Task AddBannedWord(CommandContext ctx, string bannedWord, int time)
    {
        if (!HasPermission(ctx))
        {
            await RespondWithPermissionError(ctx);
            return;
        }

        var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
        {
            var serverConfig = await _serverConfigService.GetServerConfig(ctx.Guild.Id.ToGuid());
            var bannedWords = new BannedWords
            {
                ServerConfig = serverConfig,
                BannedWord = bannedWord,
                Id = Guid.NewGuid(),
                Time = time
            };

            return await _bannedWordsService.Add(bannedWords);
        });

        await ctx.RespondAsync(result ? "Słowo dodane do bazy." : "Słowo jest już zabronione!!!");

    }
    public async Task DelBannedWord(CommandContext ctx, string delWord)
    {
        if (!HasPermission(ctx))
        {
            await RespondWithPermissionError(ctx);
            return;
        }

        await ExecuteDatabaseTaskWithResponse(ctx,
            () => _bannedWordsService.Remove(delWord, ctx.Guild.Id.ToGuid()),
            "Słowo zostało usunięte",
            "Słowo nie jest zabronione!!!");
    }

    private async Task ToggleFeature(CommandContext ctx, Func<Guid, Task> featureAction, string successMessage)
    {
        if (!HasPermission(ctx))
        {
            await RespondWithPermissionError(ctx);
            return;
        }

        await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
        {
            await featureAction(ctx.Guild.Id.ToGuid());
            return true;
        });

        await ctx.RespondAsync(successMessage);
    }
    public async Task BanEnable(CommandContext ctx) =>
        await ToggleFeature(ctx, _serverConfigService.EnableBans, "Bans Enabled");

    public async Task BanDisable(CommandContext ctx) =>
        await ToggleFeature(ctx, _serverConfigService.DisableBans, "Bans Disabled");

    public async Task KickEnable(CommandContext ctx) =>
        await ToggleFeature(ctx, _serverConfigService.EnableKicks, "Kicks Enabled");

    public async Task KickDisable(CommandContext ctx) =>
        await ToggleFeature(ctx, _serverConfigService.DisableKicks, "Kicks Disabled");

    public async Task AutoModeratorEnable(CommandContext ctx) =>
        await ToggleFeature(ctx, _serverConfigService.EnableAutoModerator, "AutoModerator Enabled");

    public async Task AutoModeratorDisable(CommandContext ctx) =>
        await ToggleFeature(ctx, _serverConfigService.DisableAutoModerator, "AutoModerator Disabled");

    public async Task AutoPlayEnabled(CommandContext ctx) =>
        await ToggleFeature(ctx, _serverConfigService.EnableAutoPlay, "AutoPlay Enabled");

    public async Task AutoPlayDisabled(CommandContext ctx) =>
        await ToggleFeature(ctx, _serverConfigService.DisableAutoPlay, "AutoPlay Disabled");

}