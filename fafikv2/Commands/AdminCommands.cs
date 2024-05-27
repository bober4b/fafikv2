using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Fafikv2.Services.CommandService;

namespace Fafikv2.Commands;

public class AdminCommands : BaseCommandModule
{
    public static AdminCommandService AdminCommandService;
    [Command("banned")]
    public async Task Banned(CommandContext ctx, string bannedWord)
    {
        await AdminCommandService.addBannedWord(ctx, bannedWord).ConfigureAwait(false);
    }
}