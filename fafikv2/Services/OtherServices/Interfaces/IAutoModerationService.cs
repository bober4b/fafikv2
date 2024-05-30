using DSharpPlus.EventArgs;

namespace Fafikv2.Services.OtherServices.Interfaces;

public interface IAutoModerationService
{
    public Task<bool> CheckMessagesAsync(MessageCreateEventArgs message);
}