using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Fafikv2.Data.DifferentClasses;
using Fafikv2.Data.Models;
using Fafikv2.Services.dbServices.Interfaces;
using Fafikv2.Services.OtherServices.Interfaces;
using Serilog;

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

        private static readonly Dictionary<int, Func<MessageCreateEventArgs, Task>> PenaltyActions = new()
        {
            { 0, Warning },
            { 1, msg => Timeout(msg, 1) },
            { 2, msg => Timeout(msg, 10) },
            { 3, msg => Timeout(msg, 60) },
            { 4, Kick }
        };


        public async Task<CheckMessagesResult> CheckMessagesAsync(MessageCreateEventArgs message)
        {
            var bannedWords = await GetBannedWordsAsync(message.Guild.Id);
            var matchingWords = bannedWords.Where(word => message.Message.Content.Contains(word.BannedWord!)).ToList();

            Log.Information("Checking message from user {User} for banned words. Found {Count} matching words.",
                message.Author.Username, matchingWords.Count);

            return new CheckMessagesResult
            {
                Result = matchingWords.Any(),
                Words = matchingWords
            };

        }
        private async Task<IEnumerable<BannedWords>> GetBannedWordsAsync(ulong guildId) =>
            await _databaseContextQueueService.EnqueueDatabaseTask(async () => await _banWordsService.GetAllByServer(guildId.ToGuid()));


        public async Task<bool> AutoModerator(MessageCreateEventArgs message)
        {
            if ((await message.Guild.GetMemberAsync(message.Author.Id)).IsOwner) return true;

            if (!await IsAutoModeratorEnabledAsync(message.Guild.Id)) return true;

            Log.Information("Auto moderation is enabled for guild {GuildId}. Checking messages.", message.Guild.Id);

            var banned = await CheckMessagesAsync(message);
            if (!banned.Result) return true;

            Log.Warning("Message from user {User} contains banned words. Applying penalties.", message.Author.Username);


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

            Log.Information("User {User} has {Penalties} penalties.", message.Author.Username, userStats.Penalties);


            await ApplyPenaltyAsync(message, userStats.Penalties);

            await _userServerStatsService.AddPenalty(message.Author.Id.ToGuid(), message.Guild.Id.ToGuid());

        }
        private async Task ApplyPenaltyAsync(MessageCreateEventArgs message, int penalties)
        {

            if (PenaltyActions.TryGetValue(penalties, out var action))
            {
                await action(message);
            }
            else
            {
                await TimeoutKickOrBan(message);
            }
        }


        private async Task<UserServerStats> GetUserStatsAsync(ulong userId, ulong guildId) =>
            (await _databaseContextQueueService.EnqueueDatabaseTask(async () => await _userServerStatsService.GetUserStats(userId.ToGuid(), guildId.ToGuid())))!;

        private async Task<bool> IsAutoModeratorEnabledAsync(ulong guildId)
        {
            return await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                var config = await _serverConfigService.GetServerConfig(guildId.ToGuid());
                return config?.AutoModeratorEnabled == true;
            });
        }

        private static async Task ApplyActionAsync(MessageCreateEventArgs message, DiscordEmbed embed, ActionType actionType)
        {
            await message.Message.DeleteAsync();
            await SendEmbedPrivateMessage(message.Guild, message.Author, embed);
            LogAction(message, actionType);
        }

        private static async Task Warning(MessageCreateEventArgs message)
        {
            
            var embed = EmbedFactory.WarningEmbed();
            await ApplyActionAsync(message, embed, ActionType.Warning);

        }

        private static async Task Timeout(MessageCreateEventArgs message, int time)
        {
            var embed = EmbedFactory.TimeoutEmbed(time);
            await ApplyActionAsync(message, embed, ActionType.Timeout);
            //await (await message.Guild.GetMemberAsync(message.Author.Id)).TimeoutAsync(DateTimeOffset.Now.AddMinutes(time));

        }

        private static async Task Kick(MessageCreateEventArgs message)
        {

            var embed = EmbedFactory.KickEmbed();
            await ApplyActionAsync(message, embed, ActionType.Kick);
            //await (await message.Guild.GetMemberAsync(message.Author.Id)).RemoveAsync();

        }
        private async Task<ServerConfig> GetServerConfigAsync(ulong guildId) =>
            (await _serverConfigService.GetServerConfig(guildId.ToGuid()))!;



        private static async Task Ban(MessageCreateEventArgs message)
        {
            
            var embed = EmbedFactory.BanEmbed();
            await ApplyActionAsync(message, embed, ActionType.Ban);
            //await (await message.Guild.GetMemberAsync(message.Author.Id)).BanAsync();
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

        private static void LogAction(MessageCreateEventArgs message, ActionType actionType)
        {
            var actionDescription = actionType switch
            {
                ActionType.Warning => "issued warning",
                ActionType.Timeout => "timed out",
                ActionType.Kick => "kicked",
                ActionType.Ban => "banned",
                _ => "performed action"
            };
            Log.Information("User {User} on guild {GuildId} has been {Action}.", message.Author.Username, message.Guild.Id, actionDescription);
        }

        private static async Task SendEmbedPrivateMessage(DiscordGuild guild, DiscordUser user, DiscordEmbed embed)
        {
            var member = await guild.GetMemberAsync(user.Id);
            var dmChannel = await member.CreateDmChannelAsync();
            await dmChannel.SendMessageAsync(embed: embed);
        }
    }

    public static class EmbedFactory
    {
        public static DiscordEmbed CreateEmbed(string title, string description, DiscordColor color) =>
            new DiscordEmbedBuilder().WithTitle(title).WithDescription(description).WithColor(color).Build();

        public static DiscordEmbed WarningEmbed() =>
            CreateEmbed("Ostrzeżenie", "Twoje ostatnie słowo było zabronione. Otrzymujesz ostrzeżenie. Kolejne wykroczenie może skutkować surowszymi konsekwencjami.", DiscordColor.Yellow);

        public static DiscordEmbed TimeoutEmbed(int time) =>
            CreateEmbed("Timeout", $"Twoje wykroczenie skutkuje timeoutem na {time} minut.", DiscordColor.Orange);

        public static DiscordEmbed KickEmbed() =>
            CreateEmbed("Wykopany z serwera", "Zostałeś wykopany z serwera. Możesz wrócić, ale uważaj na swoje zachowanie.", DiscordColor.Red);

        public static DiscordEmbed BanEmbed() =>
            CreateEmbed("Ban", "Zostałeś zbanowany z serwera.", DiscordColor.DarkRed);
    }

    public enum ActionType
    {
        Warning,
        Timeout,
        Kick,
        Ban
    }

}