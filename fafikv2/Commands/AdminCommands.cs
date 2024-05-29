﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Fafikv2.Services.CommandService;

namespace Fafikv2.Commands;

public class AdminCommands : BaseCommandModule
{
    public static AdminCommandService AdminCommandService;
    [Command("banned")]
    public async Task Banned(CommandContext ctx, string bannedWord)
    {
        await AdminCommandService.AddBannedWord(ctx, bannedWord).ConfigureAwait(false);
    }

    [Command("rbanned")]
    public async Task RBanned(CommandContext ctx, string delbanned)
    {
        await AdminCommandService.DelBannedWord(ctx, delbanned).ConfigureAwait(false);
    }
    


}