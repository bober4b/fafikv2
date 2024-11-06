using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Fafikv2.Data.DifferentClasses;
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
            var bannedWords = await GetBannedWordsAsync(message.Guild.Id);
            var matchingWords = bannedWords.Where(word => message.Message.Content.Contains(word.BannedWord!)).ToList();

            return new CheckMessagesResult
            {
                Result = matchingWords.Any(),
                Words = matchingWords
            };

        }
        private async Task<IEnumerable<BannedWords>> GetBannedWordsAsync(ulong guildId)
        {
            return await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
                await _banWordsService.GetAllByServer(guildId.ToGuid()));
        }

        public async Task<bool> AutoModerator(MessageCreateEventArgs message)
        {
            var isEnabled = await IsAutoModeratorEnabledAsync(message.Guild.Id);
            if (!isEnabled) return true;

            var banned = await CheckMessagesAsync(message);
            if (!banned.Result) return true;

            var highestPenaltyTime = banned.Words.Max(x => x.Time);

            if (banned.Words.All(x => x.Time != 0))
            {
                await Timeout(message, highestPenaltyTime);
                return false;
            }


            await HandlePenaltyAsync(message);
            return false;
        }
        private async Task HandlePenaltyAsync(MessageCreateEventArgs message)
        {
            var userStats = await GetUserStatsAsync(message.Author.Id, message.Guild.Id);

            switch (userStats.Penalties)
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

            await _userServerStatsService.AddPenalty(message.Author.Id.ToGuid(), message.Guild.Id.ToGuid());
        }
        private async Task<UserServerStats> GetUserStatsAsync(ulong userId, ulong guildId)
        {
            return (await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
                await _userServerStatsService.GetUserStats(userId.ToGuid(), guildId.ToGuid())))!;
        }
        private async Task<bool> IsAutoModeratorEnabledAsync(ulong guildId)
        {
            return await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                var config = await _serverConfigService.GetServerConfig(guildId.ToGuid());
                return config?.AutoModeratorEnabled == true;
            });
        }

        private static async Task Warning(MessageCreateEventArgs message)
        {
            await message.Message.DeleteAsync();
            await SendPrivateMessage(message.Guild, message.Author,
                "Uwaga! Twoje ostatnie słowo było zabronione. Otrzymujesz ostrzeżenie. Kolejne wykroczenie może skutkować surowszymi konsekwencjami.");
        }

        private static async Task Timeout(MessageCreateEventArgs message, int time)
        {
            await message.Message.DeleteAsync();
            await SendPrivateMessage(message.Guild, message.Author,
                $"Twoje wykroczenie skutkuje timeoutem na {time} minut.");
        }

        private async Task Kick(MessageCreateEventArgs message)
        {
            var config = await GetServerConfigAsync(message.Guild.Id);
            if (config.KicksEnabled != true) return;

            await message.Message.DeleteAsync();
            await SendPrivateMessage(message.Guild, message.Author,
                "Zostałeś wykopany z serwera. Możesz wrócić, ale uważaj na swoje zachowanie.");
        }
        private async Task<ServerConfig> GetServerConfigAsync(ulong guildId)
        {
            return (await _serverConfigService.GetServerConfig(guildId.ToGuid()))!;
        }


        private static async Task Ban(MessageCreateEventArgs message)
        {
            await message.Message.DeleteAsync();
            await SendPrivateMessage(message.Guild, message.Author, "Zostałeś zbanowany z serwera.");
        }

        private async Task TimeoutKickOrBan(MessageCreateEventArgs message)
        {
            var config = await GetServerConfigAsync(message.Guild.Id);
            var userStats = await GetUserStatsAsync(message.Author.Id, message.Guild.Id);
            var penaltyDate = userStats.LastPenaltyDate;

            if (penaltyDate == null) return;

            var differenceInDays = (DateTime.Now - penaltyDate.Value).TotalDays;

            if (differenceInDays >= 5)
            {
                await Timeout(message, 60);
            }
            else if (differenceInDays >= 2 && config.KicksEnabled)
            {
                await Kick(message);
            }
            else if (config.BansEnabled)
            {
                await Ban(message);
            }
        }

        public static async Task SendPrivateMessage(DiscordGuild guild, DiscordUser user, string message)
        {
            var member = await guild
                .GetMemberAsync(user.Id);

            var dmChannel = await member
                .CreateDmChannelAsync();

            await dmChannel
                .SendMessageAsync(message);
        }
    }

}