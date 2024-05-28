using Fafikv2.Services.dbServices.Interfaces;
using DSharpPlus.CommandsNext;
using Fafikv2.CountSystem;
using Fafikv2.Services.OtherServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;


namespace Fafikv2.Services.CommandService
{
    public class BaseCommandService
    {
        private readonly IUserService? _userService;
        private readonly IUserServerStatsService? _userServerStatsService;
        private readonly LevelSys _levelSys;
        private readonly IDatabaseContextQueueService _databaseContextQueueService;
        public BaseCommandService(IServiceProvider serviceProvider)
        {
            _userService = serviceProvider.GetRequiredService<IUserService>();
            _userServerStatsService = serviceProvider.GetRequiredService<IUserServerStatsService>();
            _databaseContextQueueService = serviceProvider.GetRequiredService<IDatabaseContextQueueService>();
            _levelSys = new();
        }

        public async Task Stats(CommandContext ctx)
        {
            if (_userService != null && _userServerStatsService != null)
            {
                await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
                {


                    var user = await _userService.GetUser(Guid.Parse($"{ctx.User.Id:X32}")).ConfigureAwait(false);
                    var userStats = await _userServerStatsService
                        .GetUserStats(Guid.Parse($"{ctx.User.Id:X32}"), Guid.Parse($"{ctx.Guild.Id:X32}"))
                        .ConfigureAwait(false);
                    if (user != null && userStats != null)
                    {
                        await ctx.RespondAsync(_levelSys.UserInfo(user, userStats)).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);


            }
        }
    }
}
