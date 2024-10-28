using DSharpPlus.CommandsNext;
using Fafikv2.CountSystem;
using Fafikv2.Data.DifferentClasses;
using Fafikv2.Services.dbServices.Interfaces;
using Fafikv2.Services.OtherServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Fafikv2.Services.CommandService
{
    public class BaseCommandService
    {
        private readonly IUserService _userService;
        private readonly IUserServerStatsService _userServerStatsService;
        private readonly LevelSys _levelSys;
        private readonly IDatabaseContextQueueService _databaseContextQueueService;
        public BaseCommandService(IServiceProvider serviceProvider)
        {
            _userService = serviceProvider.GetRequiredService<IUserService>();
            _userServerStatsService = serviceProvider.GetRequiredService<IUserServerStatsService>();
            _databaseContextQueueService = serviceProvider.GetRequiredService<IDatabaseContextQueueService>();
            _levelSys = new LevelSys();
        }

        public async Task Stats(CommandContext ctx)
        {
            await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {

                try
                {
                    var user = await _userService.GetUser(ctx.User.Id.ToGuid());
                    var userStats = await _userServerStatsService
                        .GetUserStats(ctx.User.Id.ToGuid(), ctx.Guild.Id.ToGuid());
                    if (user != null && userStats != null)
                    {
                        await ctx.RespondAsync(_levelSys.UserInfo(user, userStats));
                    }
                    else
                    {
                        await ctx.RespondAsync("Nie znaleziono danych użytkownika.");
                    }
                }
                catch (Exception ex)
                {
                    await ctx.RespondAsync("Wystąpił błąd podczas pobierania statystyk użytkownika.");
                    Log.Error(ex, "Error in Stats method for user {UserId} on guild {GuildId}"
                        , ctx.User.Id.ToGuid(), ctx.Guild.Id.ToGuid());
                }
            });
        }

        public async Task Leaderboard(CommandContext ctx)
        {

            await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                try
                {
                    var serverUserStats = (await _userServerStatsService.GetUsersStatsByServer(ctx.Guild.Id.ToGuid())).ToList();

                    if (serverUserStats.Count < 3)
                    {
                        await ctx.RespondAsync("Na serwerze jest mniej niż 3 użytkowników.");
                        return;
                    }

                    var userId = ctx.User.Id.ToGuid();
                    var userStats = await _userServerStatsService.GetUserStats(userId, ctx.Guild.Id.ToGuid());
                    var index = serverUserStats.FindIndex(x => x.Id == userStats?.Id);

                    var response = string.Format(
                        "Najbardziej aktywni użytkownicy:\n" +
                        "1. {0}: {1}\n" +
                        "2. {2}: {3}\n" +
                        "3. {4}: {5}\n\n" +
                        "Twoja Pozycja: {6}\n" +
                        "{7}: {8}",
                        serverUserStats[0].DisplayName, serverUserStats[0].MessagesCountServer,
                        serverUserStats[1].DisplayName, serverUserStats[1].MessagesCountServer,
                        serverUserStats[2].DisplayName, serverUserStats[2].MessagesCountServer,
                        index + 1, userStats?.DisplayName, userStats?.MessagesCountServer
                    );

                    await ctx.RespondAsync(response);
                }
                catch (Exception ex)
                {
                    await ctx.RespondAsync("Wystąpił błąd podczas pobierania tabeli wyników.");
                    Log.Error(ex, "Error in Leaderboard method for guild {GuildId}", ctx.Guild.Id.ToGuid());
                }
            });
        }
    }
}
