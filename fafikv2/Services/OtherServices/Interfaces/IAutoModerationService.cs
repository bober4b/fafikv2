using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Fafikv2.Services.OtherServices.Interfaces;

public interface IAutoModerationService
{
    public void ClientConnect(DiscordClient client);
    public Task<bool> CheckMessagesAsync(MessageCreateEventArgs message);
    public Task<bool> AutoModerator(MessageCreateEventArgs message);
}