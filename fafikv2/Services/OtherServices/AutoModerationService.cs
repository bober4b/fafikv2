using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Fafikv2.Data.Models;
using Fafikv2.Services.dbServices.Interfaces;
using Fafikv2.Services.OtherServices.Interfaces;

namespace Fafikv2.Services.OtherServices
{
    public class AutoModerationService : IAutoModerationService
    {
        private readonly IDatabaseContextQueueService _databaseContextQueueService;
        private readonly IBannedWordsService _banWordsService;
        private readonly IUserServerStatsService _userServerStatsService;
        private readonly IServerConfigService _serverConfigService;

        public AutoModerationService(IDatabaseContextQueueService databaseContextQueueService,
            IBannedWordsService bannedWordsService,
            IUserServerStatsService userServerStatsService,
            IServerConfigService serverConfigService)
        {
            _databaseContextQueueService = databaseContextQueueService;
            _banWordsService = bannedWordsService;
            _userServerStatsService = userServerStatsService;
            _serverConfigService = serverConfigService;
        }


        public async Task<CheckMessagesResult> CheckMessagesAsync(MessageCreateEventArgs message)
        {

            var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                var badWords = await _banWordsService
                    .GetAllByServer(Guid.Parse($"{message.Guild.Id:X32}"))
                     ;

                var bannedWordsEnumerable = badWords as BannedWords[] ?? badWords.ToArray();

                return bannedWordsEnumerable
                    .Any(word => word.BannedWord != null && message.Message.Content
                        .Contains(word.BannedWord));
            });

            if (!result)
                return new CheckMessagesResult
                {
                    Result = result,
                    Words = Enumerable.Empty<BannedWords>()
                };

            var words = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                var badWords = await _banWordsService
                    .GetAllByServer(Guid.Parse($"{message.Guild.Id:X32}"))
                     ;

                var containWords = badWords
                    .Where(word => word.BannedWord != null && message.Message.Content.Contains(word.BannedWord));

                return containWords;
            });

            return new CheckMessagesResult()
            {
                Result = result,
                Words = words
            };

        }

        public async Task<bool> AutoModerator(MessageCreateEventArgs message)
        {
            var enable = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                var config = await _serverConfigService
                    .GetServerConfig(Guid.Parse($"{message.Guild.Id:X32}"))
                     ;


                return config is { AutoModeratorEnabled: true };
            });

            if (!enable) return true;
            var banned = await CheckMessagesAsync(message);

            if (!banned.Result) return true;

            if (banned.Words.All(x => x.Time != 0))
            {
                var timeout = banned.Words.Max(x => x.Time);
                await Timeout(message, timeout);
                return false;
            }


            await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                var user = await _userServerStatsService
                    .GetUserStats(Guid.Parse($"{message.Author.Id:X32}"),
                        Guid.Parse($"{message.Guild.Id:X32}"))
                     ;

                if (user == null) return false;
                switch (user.Penalties)
                {
                    case 0:
                        await Warning(message);
                        break;
                    case 1:
                        await Timeout(message, 1);
                        break;
                    case 2:
                        await Timeout(message, 10);
                        break;
                    case 3:
                        await Timeout(message, 60);
                        break;
                    case 4:
                        await Kick(message);
                        break;
                    default:
                        await TimeoutKickOrBan(message);
                        break;

                }


                await _userServerStatsService
                    .AddPenalty(Guid.Parse($"{message.Author.Id:X32}"),
                        Guid.Parse($"{message.Guild.Id:X32}"));

                return true;
            });

            return false;
        }

        private static async Task Warning(MessageCreateEventArgs message)
        {
            await message.Message.DeleteAsync();
            await SendPrivateMessage(message.Guild,
                    message.Author,
                    "folguj się kolego, te słowo które napisałeś było zakazane, otrzymujesz pierwsze " +
                    "ostrzeżenie, ale kolejne złamanie regulaminu może mieć większe konsekwencje.")
                 ;
        }

        private static async Task Timeout(MessageCreateEventArgs message, int time)
        {
            DateTimeOffset nowTimeOffset = DateTimeOffset.Now;
            DateTimeOffset timeout = nowTimeOffset.AddMinutes(time);



            var member = await message.Guild.GetMemberAsync(message.Author.Id);



            //await member.TimeoutAsync(timeout) ;

            await message.Message.DeleteAsync();
            await SendPrivateMessage(message.Guild,
                    message.Author,
                    $"kolego, to twoje kolejne ostrzeżenie. tym razem idziesz na przerwę na {time} minut")
                 ;
        }

        private async Task Kick(MessageCreateEventArgs message)
        {
            var config = await _serverConfigService.GetServerConfig(Guid.Parse($"{message.Guild.Id:X32}"))
                 ;
            if (config is { KicksEnabled: false }) return;
            //var member = await message.Guild.GetMemberAsync(message.Author.Id) ;
            await message.Message.DeleteAsync();

            // await member.RemoveAsync() ;

            await SendPrivateMessage(message.Guild,
                    message.Author,
                    $"Pofolgowaliście sobie kolego?, " +
                    $"tym razem dostałeś tylko kick, dalej możecie dołączyć do serwera poprzez zaproszenie, " +
                    $"ale kolejne złamanie regulaminu może zakończyć się banicją, więc uważaj na przyszłości, bo pożegnamy się na dłużej. ")
                 ;
        }

        private static async Task Ban(MessageCreateEventArgs message)
        {
            var member = await message.Guild.GetMemberAsync(message.Author.Id);
            await message.Message.DeleteAsync();

            //await member.BanAsync() ;

            await SendPrivateMessage(message.Guild,
                    message.Author,
                    "Oj kolego, teraz to już przesadziłeś. Twoja ostatnia wiadomość zakończyła się banicją, teraz to masz problem," +
                    " proś o wybaczenie, ale nie wiem czy zda się to ci na wiele.  ")
                 ;
        }

        private async Task TimeoutKickOrBan(MessageCreateEventArgs message)
        {
            var config = await _serverConfigService.GetServerConfig(Guid.Parse($"{message.Guild.Id:X32}"))
                 ;



            var user = await _userServerStatsService
                .GetUserStats(Guid.Parse($"{message.Author.Id:X32}"),
                    Guid.Parse($"{message.Guild.Id:X32}"))
                 ;

            var date = user?.LastPenaltyDate;

            if (date == null) return;
            var difference = DateTime.Now - date;

            switch (difference.Value.TotalDays)
            {
                case >= 5:
                    await Timeout(message, 60);
                    return;
                case >= 2:
                    if (config is { KicksEnabled: true }) return;
                    await Kick(message);
                    return;
                default:
                    if (config is { BansEnabled: true }) return;
                    await Ban(message);
                    return;

            }


        }

        public static async Task SendPrivateMessage(DiscordGuild guild, DiscordUser user, string message)
        {
            var member = await guild
                .GetMemberAsync(user.Id)
                 ;

            var dmChannel = await member
                .CreateDmChannelAsync()
                 ;


            await dmChannel
                .SendMessageAsync(message)
                 ;
        }
    }

}