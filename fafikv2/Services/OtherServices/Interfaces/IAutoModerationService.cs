using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Fafikv2.Services.OtherServices.Interfaces;

public interface IAutoModerationService
{
    public Task<CheckMessagesResult> CheckMessagesAsync(MessageCreateEventArgs message);
    public Task<bool> AutoModerator(MessageCreateEventArgs message);
}