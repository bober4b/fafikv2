using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Fafikv2.Services.CommandService;

namespace Fafikv2.Commands;

public class AdminCommands : BaseCommandModule
{
    public static AdminCommandService? AdminCommandService;

    [Command("banned")]
    public async Task Banned(CommandContext ctx, string bannedWord, int time=0)
    {
        await AdminCommandService.AddBannedWord(ctx, bannedWord, time).ConfigureAwait(false);
    }

    [Command("rbanned")]
    public async Task RBanned(CommandContext ctx, string delbanned)
    {
        await AdminCommandService.DelBannedWord(ctx, delbanned).ConfigureAwait(false);
    }

    [Command("kick_enable")]
    public async Task KickEnable(CommandContext ctx)
    {
       await AdminCommandService.KickEnable(ctx);
    }

    [Command("kick_disable")]
    public async Task KickDisable(CommandContext ctx)
    {
        await AdminCommandService.KickDisable(ctx);
    }

    [Command("ban_enable")]
    public async Task BansEnabled(CommandContext ctx)
    {
        await AdminCommandService.BanEnable(ctx);
    }

    [Command("ban_disable")]
    public async Task BansDisable(CommandContext ctx)
    {
        await AdminCommandService.BanDisable(ctx);
    }
    [Command("Auto_moderator_enable")]
    public async Task AutoModeratorEnable(CommandContext ctx)
    {
        await AdminCommandService.AutoModeratorEnable(ctx);
    }
    [Command("Auto_moderator_disable")]
    public async Task AutoModeratorDisable(CommandContext ctx)
    {
        await AdminCommandService.AutoModeratorDisable(ctx);
    }
    [Command("Auto_play_enable")]
    public async Task AutoPlayEnable(CommandContext ctx)
    {
        await AdminCommandService.AutoPlayEnabled(ctx);
    }
    [Command("Auto_play_disable")]
    public async Task AutoPlayDisable(CommandContext ctx)
    {
        await AdminCommandService.AutoPlayDisabled(ctx);
    }

}