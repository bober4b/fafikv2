using DSharpPlus.EventArgs;
using Fafikv2.Services.dbServices.Interfaces;
using Fafikv2.Services.OtherServices.Interfaces;

namespace Fafikv2.Services.OtherServices;

public class AutoModerationService : IAutoModerationService
{
    private readonly IDatabaseContextQueueService _databaseContextQueueService;
    private readonly IBannedWordsService _banWordsService;

    public AutoModerationService(IDatabaseContextQueueService databaseContextQueueService,
        IBannedWordsService bannedWordsService)
    {
        _databaseContextQueueService = databaseContextQueueService;
        _banWordsService = bannedWordsService;
    }

    public async Task<bool> CheckMessagesAsync(MessageCreateEventArgs message)
    {
        
        var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
        {
            var badWords = await _banWordsService
                .GetAll(Guid.Parse($"{message.Guild.Id:X32}"))
                .ConfigureAwait(false);

            return badWords
                .Any(word => message.Message.Content
                    .Contains(word.BannedWord));
        }).ConfigureAwait(false);

        return result;
    }
}