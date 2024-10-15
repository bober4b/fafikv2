using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Fafikv2.Services.CommandService;

namespace Fafikv2.Commands;

public class AdminCommands : BaseCommandModule
{
    public static AdminCommandService CommandService { get; set; } = null!;

    [Command("banned")]
    public async Task Banned(CommandContext ctx, string bannedWord, int time=0)
    {
        await CommandService.AddBannedWord(ctx, bannedWord, time) ;
    }

    [Command("rbanned")]
    public async Task RBanned(CommandContext ctx, string delbanned)
    {
        await CommandService.DelBannedWord(ctx, delbanned) ;
    }

    [Command("kick_enable")]
    public async Task KickEnable(CommandContext ctx)
    {
       await CommandService.KickEnable(ctx);
    }

    [Command("kick_disable")]
    public async Task KickDisable(CommandContext ctx)
    {
        await CommandService.KickDisable(ctx);
    }

    [Command("ban_enable")]
    public async Task BansEnabled(CommandContext ctx)
    {
        await CommandService.BanEnable(ctx);
    }

    [Command("ban_disable")]
    public async Task BansDisable(CommandContext ctx)
    {
        await CommandService.BanDisable(ctx);
    }
    [Command("auto_moderator_enable")]
    public async Task AutoModeratorEnable(CommandContext ctx)
    {
        await CommandService.AutoModeratorEnable(ctx);
    }
    [Command("auto_moderator_disable")]
    public async Task AutoModeratorDisable(CommandContext ctx)
    {
        await CommandService.AutoModeratorDisable(ctx);
    }
    [Command("auto_play_enable")]
    public async Task AutoPlayEnable(CommandContext ctx)
    {
        await CommandService.AutoPlayEnabled(ctx);
    }
    [Command("auto_play_disable")]
    public async Task AutoPlayDisable(CommandContext ctx)
    {
        await CommandService.AutoPlayDisabled(ctx);
    }

}