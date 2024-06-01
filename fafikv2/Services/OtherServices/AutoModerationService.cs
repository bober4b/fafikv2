using DSharpPlus.EventArgs;
using Fafikv2.Services.dbServices.Interfaces;
using Fafikv2.Services.OtherServices.Interfaces;

namespace Fafikv2.Services.OtherServices;

public class AutoModerationService : IAutoModerationService
{
    private readonly IDatabaseContextQueueService _databaseContextQueueService;
    private readonly IBannedWordsService _banWordsService;
    private readonly IUserServerStatsService _userServerStatsService;

    public AutoModerationService(IDatabaseContextQueueService databaseContextQueueService,
        IBannedWordsService bannedWordsService,
        IUserServerStatsService userServerStatsService)
    {
        _databaseContextQueueService = databaseContextQueueService;
        _banWordsService = bannedWordsService;
        _userServerStatsService = userServerStatsService;
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
        //var cos=await AutoModerator(message).ConfigureAwait(false);


        return result;
        
    }

    public async Task<bool> AutoModerator(MessageCreateEventArgs message)
    {
        Console.WriteLine("Data ostatniej kary jeszcze nie istnieje");
        //var result = await CheckMessagesAsync(message).ConfigureAwait(false);
        //if (result) return true;

        var userStats = await _userServerStatsService
            .GetUserStats(Guid.Parse($"{message.Author.Id:X32}"),
                Guid.Parse($"{message.Guild.Id:X32}")).ConfigureAwait(false);

        if (userStats == null)
        {
            Console.WriteLine("Błąd, użytkownik nie istnieje!!");
            return false;
        }

        Console.WriteLine(userStats.LastPenaltyDate != null
            ? $"Ostatnia data kary: {userStats.LastPenaltyDate}"
            : "Data ostatniej kary jeszcze nie istnieje");

        await _userServerStatsService
            .AddPenalty(Guid.Parse($"{message.Author.Id:X32}"),
                Guid.Parse($"{message.Guild.Id:X32}")).ConfigureAwait(false);

        // Pobierz statystyki użytkownika ponownie po dodaniu kary, aby upewnić się, że dane są aktualne
        userStats = await _userServerStatsService
            .GetUserStats(Guid.Parse($"{message.Author.Id:X32}"),
                Guid.Parse($"{message.Guild.Id:X32}")).ConfigureAwait(false);

        Console.WriteLine(userStats?.LastPenaltyDate != null
            ? $"Nowa data ostatniej kary: {userStats.LastPenaltyDate}"
            : "Data ostatniej kary nadal nie istnieje");

        return true;
    }

}