using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus;
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
        private static DiscordClient _client;

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

        public void ClientConnect(DiscordClient client)
        {
            _client = client;
        }

        public async Task<CheckMessagesResult> CheckMessagesAsync(MessageCreateEventArgs message)
        {

            var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                var badWords = await _banWordsService
                    .GetAllByServer(Guid.Parse($"{message.Guild.Id:X32}"))
                    .ConfigureAwait(false);

                var ces = badWords.ToArray();

                return badWords
                    .Any(word => message.Message.Content
                        .Contains(word.BannedWord));
            }).ConfigureAwait(false);

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
                    .ConfigureAwait(false);

                var containWords = badWords
                    .Where(word => message.Message.Content.Contains(word.BannedWord));

                return containWords;
            }).ConfigureAwait(false);

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
                    .ConfigureAwait(false);


                return config.AutoModeratorEnabled;
            }).ConfigureAwait(false);

            if (!enable) return true;
            var banned = await CheckMessagesAsync(message).ConfigureAwait(false);

            if (!banned.Result) return true;

            if (banned.Words.All(x => x.Time != 0))
            {
                var timeout = banned.Words.Max(x => x.Time);
                await Timeout(message, timeout).ConfigureAwait(false);
                return false;
            }


            var penalty = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                var user = await _userServerStatsService
                    .GetUserStats(Guid.Parse($"{message.Author.Id:X32}"),
                        Guid.Parse($"{message.Guild.Id:X32}"))
                    .ConfigureAwait(false);

                if (user == null) return false;
                switch (user.Penalties)
                {
                    case 0:
                        await Warning(message).ConfigureAwait(false);
                        break;
                    case 1:
                        await Timeout(message, 1).ConfigureAwait(false);
                        break;
                    case 2:
                        await Timeout(message, 10).ConfigureAwait(false);
                        break;
                    case 3:
                        await Timeout(message, 60).ConfigureAwait(false);
                        break;
                    case 4:
                        await Kick(message).ConfigureAwait(false);
                        break;
                    default:
                        await TimeoutKickOrBan(message).ConfigureAwait(false);
                        break;

                }


                await _userServerStatsService
                    .AddPenalty(Guid.Parse($"{message.Author.Id:X32}"),
                        Guid.Parse($"{message.Guild.Id:X32}")).ConfigureAwait(false);

                return true;
            }).ConfigureAwait(false);

            return false;
        }

        private static async Task Warning(MessageCreateEventArgs message)
        {
            await message.Message.DeleteAsync().ConfigureAwait(false);
            await SendPrivateMessage(message.Guild,
                    message.Author,
                    "folguj się kolego, te słowo które napisałeś było zakazane, otrzymujesz pierwsze " +
                    "ostrzeżenie, ale kolejne złamanie regulaminu może mieć większe konsekwencje.")
                .ConfigureAwait(false);
        }

        private static async Task Timeout(MessageCreateEventArgs message, int time)
        {
            DateTimeOffset nowTimeOffset = DateTimeOffset.Now;
            DateTimeOffset timeout = nowTimeOffset.AddMinutes(time);



            var member = await message.Guild.GetMemberAsync(message.Author.Id).ConfigureAwait(false);



            //await member.TimeoutAsync(timeout).ConfigureAwait(false);

            await message.Message.DeleteAsync().ConfigureAwait(false);
            await SendPrivateMessage(message.Guild,
                    message.Author,
                    $"kolego, to twoje kolejne ostrzeżenie. tym razem idziesz na przerwę na {time} minut")
                .ConfigureAwait(false);
        }

        private async Task Kick(MessageCreateEventArgs message)
        {
            var config = await _serverConfigService.GetServerConfig(Guid.Parse($"{message.Guild.Id:X32}"))
                .ConfigureAwait(false);
            if (!config.KicksEnabled) return;
            //var member = await message.Guild.GetMemberAsync(message.Author.Id).ConfigureAwait(false);
            await message.Message.DeleteAsync().ConfigureAwait(false);

            // await member.RemoveAsync().ConfigureAwait(false);

            await SendPrivateMessage(message.Guild,
                    message.Author,
                    $"Pofolgowaliście sobie kolego?, " +
                    $"tym razem dostałeś tylko kick, dalej możecie dołączyć do serwera poprzez zaproszenie, " +
                    $"ale kolejne złamanie regulaminu może zakończyć się banicją, więc uważaj na przyszłości, bo pożegnamy się na dłużej. ")
                .ConfigureAwait(false);
        }

        private static async Task Ban(MessageCreateEventArgs message)
        {
            var member = await message.Guild.GetMemberAsync(message.Author.Id).ConfigureAwait(false);
            await message.Message.DeleteAsync().ConfigureAwait(false);

            //await member.BanAsync().ConfigureAwait(false);

            await SendPrivateMessage(message.Guild,
                    message.Author,
                    "Oj kolego, teraz to już przesadziłeś. Twoja ostatnia wiadomość zakończyła się banicją, teraz to masz problem," +
                    " proś o wybaczenie, ale nie wiem czy zda się to ci na wiele.  ")
                .ConfigureAwait(false);
        }

        private async Task TimeoutKickOrBan(MessageCreateEventArgs message)
        {
            var config = await _serverConfigService.GetServerConfig(Guid.Parse($"{message.Guild.Id:X32}"))
                .ConfigureAwait(false);



            var user = await _userServerStatsService
                .GetUserStats(Guid.Parse($"{message.Author.Id:X32}"),
                    Guid.Parse($"{message.Guild.Id:X32}"))
                .ConfigureAwait(false);
            if (user == null) return;

            var date = user.LastPenaltyDate;

            if (date == null) return;
            var difference = DateTime.Now - date;

            switch (difference.Value.TotalDays)
            {
                case >= 5:
                    await Timeout(message, 60).ConfigureAwait(false);
                    return;
                case >= 2:
                    if (config.KicksEnabled) return;
                    await Kick(message).ConfigureAwait(false);
                    return;
                default:
                    if (config.BansEnabled) return;
                    await Ban(message).ConfigureAwait(false);
                    return;

            }


        }

        public static async Task SendPrivateMessage(DiscordGuild guild, DiscordUser user, string message)
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

}