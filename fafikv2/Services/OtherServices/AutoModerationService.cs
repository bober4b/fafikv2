using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus;
using Fafikv2.Services.dbServices.Interfaces;
using Fafikv2.Services.OtherServices.Interfaces;

namespace Fafikv2.Services.OtherServices;

public class AutoModerationService : IAutoModerationService
{
    private readonly IDatabaseContextQueueService _databaseContextQueueService;
    private readonly IBannedWordsService _banWordsService;
    private readonly IUserServerStatsService _userServerStatsService;
    private static DiscordClient _client;

    public AutoModerationService(IDatabaseContextQueueService databaseContextQueueService,
        IBannedWordsService bannedWordsService,
        IUserServerStatsService userServerStatsService)
    {
        _databaseContextQueueService = databaseContextQueueService;
        _banWordsService = bannedWordsService;
        _userServerStatsService = userServerStatsService;
    }

    public void ClientConnect(DiscordClient client)
    {
        _client=client;
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

        if (result)
        {
            //await Warning(message).ConfigureAwait(false);
            await Timeout(message, 1).ConfigureAwait(false);
        }
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


        //await SendPrivateMessage(message.Guild, message.Author, "moonwalk like michael").ConfigureAwait(false);

        return true;
    }

    public static async Task Warning(MessageCreateEventArgs message)
    {
        await message.Message.DeleteAsync().ConfigureAwait(false);
        await SendPrivateMessage(message.Guild,
                message.Author,
                "folguj się kolego, te słowo które napisałeś było zakazane, otrzymujesz pierwsze " +
                "ostrzeżenie, ale kolejne złamanie regulaminu może mieć większe konsekwencje.")
            .ConfigureAwait(false);
    }

    public async Task Timeout(MessageCreateEventArgs message,int time)
    {
        DateTimeOffset nowTimeOffset=DateTimeOffset.Now;
        DateTimeOffset timeout=nowTimeOffset.AddMinutes(time);
        
        var member = await message.Guild.GetMemberAsync(message.Author.Id).ConfigureAwait(false);

        await member.TimeoutAsync(timeout).ConfigureAwait(false);

        await message.Message.DeleteAsync().ConfigureAwait(false);
        await SendPrivateMessage(message.Guild,
                message.Author,
                $"kolego, to twoje kolejne ostrzeżenie. tym razem idziesz na przerwę na {time}")
            .ConfigureAwait(false);
    }

    public static async Task SendPrivateMessage(DiscordGuild guild ,DiscordUser user, string message)
    {
        var member = await guild
            .GetMemberAsync(user.Id)
            .ConfigureAwait(false);
        
        var dmChannel = await member
            .CreateDmChannelAsync()
            .ConfigureAwait(false);


        await dmChannel
            .SendMessageAsync(message)
            .ConfigureAwait(false);
    }
}