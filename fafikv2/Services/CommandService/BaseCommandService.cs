using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fafikv2.Data.Models;
using Fafikv2.Services.dbServices.Interfaces;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Fafikv2.CountSystem;

namespace Fafikv2.Services.CommandService
{
    public class BaseCommandService
    {
        private readonly IUserService _userService;
        private readonly IUserServerStatsService _userServerStatsService;
        private readonly LevelSys _levelSys;
        public BaseCommandService(IUserService userService, IUserServerStatsService userServerStatsService)
        {
            _userService = userService;
            _userServerStatsService = userServerStatsService;
            _levelSys = new();
        }

        public async Task Stats(CommandContext ctx)
        {
            var guildId = ctx.Channel.Guild.Id;

            var dcuser = ctx.Member.Id;
            var dcserver = ctx.Guild.Id;
        }
    }
}
