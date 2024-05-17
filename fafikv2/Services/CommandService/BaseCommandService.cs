using Fafikv2.Services.dbServices.Interfaces;
using DSharpPlus.CommandsNext;
using Fafikv2.CountSystem;


namespace Fafikv2.Services.CommandService
{
    public class BaseCommandService
    {
        private readonly IUserService? _userService;
        private readonly IUserServerStatsService? _userServerStatsService;
        private readonly LevelSys _levelSys;
        public BaseCommandService(IUserService? userService, IUserServerStatsService? userServerStatsService)
        {
            _userService = userService;
            _userServerStatsService = userServerStatsService;
            _levelSys = new();
        }

        public async Task Stats(CommandContext ctx)
        {
            if (_userService != null && _userServerStatsService != null)
            {
                var user = await _userService.GetUser(Guid.Parse($"{ctx.User.Id:X32}")).ConfigureAwait(false);
                var userStats = await _userServerStatsService.GetUserStats(Guid.Parse($"{ctx.User.Id:X32}"), Guid.Parse($"{ctx.Guild.Id:X32}")).ConfigureAwait(false);
                if (user !=null && userStats !=null)
                {
                    await ctx.Channel.SendMessageAsync( _levelSys.UserInfo(user, userStats)).ConfigureAwait(false);
                }
                
                
            }
        }
    }
}
