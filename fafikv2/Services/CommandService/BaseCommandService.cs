using Fafikv2.Services.dbServices.Interfaces;
using DSharpPlus.CommandsNext;
using Fafikv2.CountSystem;
using Fafikv2.Services.OtherServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;


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


                var user = await _userService.GetUser(Guid.Parse($"{ctx.User.Id:X32}")) ;
                var userStats = await _userServerStatsService
                        .GetUserStats(Guid.Parse($"{ctx.User.Id:X32}"), Guid.Parse($"{ctx.Guild.Id:X32}"))
                    ;
                if (user != null && userStats != null)
                {
                    await ctx.RespondAsync(_levelSys.UserInfo(user, userStats)) ;
                        
                }
            }) ;
        }

        public async Task Leaderboard(CommandContext ctx)
        {
            await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                var serverUserStats = (await _userServerStatsService
                    .GetUsersStatsByServer(Guid.Parse($"{ctx.Guild.Id:X32}"))
                     ).ToList();

                if (serverUserStats.Count < 3)
                {
                    await ctx.RespondAsync("Na serwerze jest mniej niż 3 użytkowników.") ;
                    return;
                }

                var userStats = await _userServerStatsService
                    .GetUserStats(Guid.Parse($"{ctx.User.Id:X32}"), Guid.Parse($"{ctx.Guild.Id:X32}"))
                     ;

                var index = serverUserStats.FindIndex(x => x.Id == userStats?.Id);

                var response=($"Najbardziej aktywni użytkownicy:\n" +
                                 $"1. {serverUserStats[0].DisplayName}: {serverUserStats[0].MessagesCountServer}\n" +
                                 $"2. {serverUserStats[1].DisplayName}: {serverUserStats[1].MessagesCountServer}\n" +
                                 $"3. {serverUserStats[2].DisplayName}: {serverUserStats[2].MessagesCountServer}\n\n" +
                                 $"Twoja Pozycja: {index+1}\n" +
                                 $"{userStats?.DisplayName}: {userStats?.MessagesCountServer}"
                                 );
               await ctx.RespondAsync(response) ;

            }) ;


        }
    }
}
