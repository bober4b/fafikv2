using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Fafikv2.Data.DifferentClasses;
using Fafikv2.Data.Models;
using Fafikv2.Services.dbServices.Interfaces;
using Fafikv2.Services.OtherServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Fafikv2.Services.CommandService
{
    public class BaseCommandService
    {
        private readonly IUserService _userService;
        private readonly IUserServerStatsService _userServerStatsService;
        private readonly IDatabaseContextQueueService _databaseContextQueueService;

        public BaseCommandService(IServiceProvider serviceProvider)
        {
            _userService = serviceProvider.GetRequiredService<IUserService>();
            _userServerStatsService = serviceProvider.GetRequiredService<IUserServerStatsService>();
            _databaseContextQueueService = serviceProvider.GetRequiredService<IDatabaseContextQueueService>();
        }

        public async Task Stats(CommandContext ctx)
        {
            await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                try
                {
                    var user = await _userService.GetUser(ctx.User.Id.ToGuid());
                    var userStats = await _userServerStatsService
                        .AsNoTracking(ctx.User.Id.ToGuid(), ctx.Guild.Id.ToGuid());

                    if (user != null && userStats != null)
                    {
                        var embed = CreateUserInfoEmbed(user, userStats);
                        await ctx.RespondAsync(embed);
                    }
                    else
                    {
                        await ctx.RespondAsync("Nie znaleziono danych użytkownika.");
                    }
                }
                catch (Exception ex)
                {
                    await ctx.RespondAsync("Wystąpił błąd podczas pobierania statystyk użytkownika.");
                    Log.Error(ex, "Error in Stats method for user {UserId} on guild {GuildId}", ctx.User.Id.ToGuid(), ctx.Guild.Id.ToGuid());
                }
            });
        }

        public async Task Leaderboard(CommandContext ctx)
        {
            await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                try
                {
                    var serverUserStats = (await _userServerStatsService.GetUserStatsByServerOnlyToRead(ctx.Guild.Id.ToGuid())).ToList();

                    if (serverUserStats.Count < 3)
                    {
                        await ctx.RespondAsync("Na serwerze jest mniej niż 3 użytkowników.");
                        return;
                    }

                    var userId = ctx.User.Id.ToGuid();
                    var userStats = await _userServerStatsService.AsNoTracking(userId, ctx.Guild.Id.ToGuid());

                    if (userStats == null)
                    {
                        await ctx.RespondAsync("Nie znaleziono twoich statystyk.");
                        return;
                    }

                    var index = serverUserStats.FindIndex(x => x.Id == userStats.Id);
                    var embed = CreateLeaderboardEmbed(serverUserStats, userStats, index);

                    await ctx.RespondAsync(embed);
                }
                catch (Exception ex)
                {
                    await ctx.RespondAsync("Wystąpił błąd podczas pobierania tabeli wyników.");
                    Log.Error(ex, "Error in Leaderboard method for guild {GuildId}", ctx.Guild.Id.ToGuid());
                }
            });
        }

        private static DiscordEmbed CreateUserInfoEmbed(User user, UserServerStats userServerStats)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"{userServerStats.DisplayName} - Statystyki",
                Color = DiscordColor.Blurple
            }
            .AddField("Wysłane wiadomości (globalnie)", $"`{user.MessagesCountGlobal}`", true)
            .AddField("Interakcje z botem (globalnie)", $"`{user.BotInteractionGlobal}`",true)
            .AddField("\u200B", "\u200B", true) 
            .AddField("Wysłane wiadomości (serwer)", $"`{userServerStats.MessagesCountServer}`",true)
            .AddField("Interakcje z botem (serwer)", $"`{userServerStats.BotInteractionServer}`", true)
            .AddField("\u200B", "\u200B", true);

            return embed;
        }

        private DiscordEmbed CreateLeaderboardEmbed(
            List<UserServerStats> serverUserStats,
            UserServerStats userStats,
            int index)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Najbardziej aktywni użytkownicy",
                Color = DiscordColor.Green
            }
            .AddField("1. " + serverUserStats[0].DisplayName, $"`{serverUserStats[0].MessagesCountServer}` wiadomości")
            .AddField("2. " + serverUserStats[1].DisplayName, $"`{serverUserStats[1].MessagesCountServer}` wiadomości")
            .AddField("3. " + serverUserStats[2].DisplayName, $"`{serverUserStats[2].MessagesCountServer}` wiadomości")
            .AddField("Twoja pozycja", $"`{index + 1}.` {userStats.DisplayName}: `{userStats.MessagesCountServer}` wiadomości");

            return embed;
        }
    }
}
