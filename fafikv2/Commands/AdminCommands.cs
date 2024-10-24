﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Fafikv2.Services.CommandService;

namespace Fafikv2.Commands;

public class AdminCommands : BaseCommandModule
{
    private readonly AdminCommandService _commandService;

    public AdminCommands(AdminCommandService commandService)
    {
        _commandService=commandService;
    }

    [Command("banned")]
    public async Task Banned(CommandContext ctx, string bannedWord, int time = 0)
    {
        await _commandService.AddBannedWord(ctx, bannedWord, time);
    }

    [Command("rbanned")]
    public async Task RBanned(CommandContext ctx, string delbanned)
    {
        await _commandService.DelBannedWord(ctx, delbanned);
    }

    [Command("kick_enable")]
    public async Task KickEnable(CommandContext ctx)
    {
        await _commandService.KickEnable(ctx);
    }

    [Command("kick_disable")]
    public async Task KickDisable(CommandContext ctx)
    {
        await _commandService.KickDisable(ctx);
    }

    [Command("ban_enable")]
    public async Task BansEnabled(CommandContext ctx)
    {
        await _commandService.BanEnable(ctx);
    }

    [Command("ban_disable")]
    public async Task BansDisable(CommandContext ctx)
    {
        await _commandService.BanDisable(ctx);
    }
    [Command("auto_moderator_enable")]
    public async Task AutoModeratorEnable(CommandContext ctx)
    {
        await _commandService.AutoModeratorEnable(ctx);
    }
    [Command("auto_moderator_disable")]
    public async Task AutoModeratorDisable(CommandContext ctx)
    {
        await _commandService.AutoModeratorDisable(ctx);
    }
    [Command("auto_play_enable")]
    public async Task AutoPlayEnable(CommandContext ctx)
    {
        await _commandService.AutoPlayEnabled(ctx);
    }
    [Command("auto_play_disable")]
    public async Task AutoPlayDisable(CommandContext ctx)
    {
        await _commandService.AutoPlayDisabled(ctx);
    }

}