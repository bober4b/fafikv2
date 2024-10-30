using DSharpPlus;
using DSharpPlus.Entities;
using Fafikv2.Data.Models;

namespace Fafikv2.Commands.MessageCreator
{
    public static class MessagesComposition
    {
        public static DiscordMessageBuilder EmbedPanelComposition(SongServiceDictionary songServiceDictionary)
        {

            var duration = songServiceDictionary.Queue?.Count > 0
                ? songServiceDictionary.Queue[0].Length.ToString(@"mm\:ss")
                : "0:00";

            var queue = songServiceDictionary.Queue?.Count > 0 ? songServiceDictionary.Queue.Count.ToString() : "0";

            var embed = new DiscordEmbedBuilder
                {
                    Title = "NOW PLAYING: " + (songServiceDictionary.Queue?.Count > 0 ? songServiceDictionary.Queue[0].Title : "No track playing"),
                    Color = new DiscordColor(2326507)
                }
                .AddField("Duration",$"`{duration}`", true)
                .AddField("Queue", $"`{queue}`", true)
                .AddField("Volume", $"`{songServiceDictionary.Volume}%`", true)
                .Build();

            

            return new DiscordMessageBuilder()
                .WithEmbed(embed)
                .AddComponents(
                    new DiscordButtonComponent(ButtonStyle.Secondary, "Play", "Play", false,
                        new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶️"))),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "Skip", "Skip", false,
                        new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏭️"))),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "Pause", "Pause", false,
                        new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏸️")))
                )
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, "Auto_Play", "Auto Play", false,
                        new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔀"))),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "Queue", "Queue", false,
                        new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🎦")))
                );

        }

        public static DiscordEmbed EmbedQueueComposition(string songs)
        {
            return new DiscordEmbedBuilder
            {
                Title = "QUEUE: ",
                Color = new DiscordColor(2326507),
                Description = songs
            }.Build();
        }

        public static DiscordEmbed EmbedMessageComposition(string title, string description)
        {
            return new DiscordEmbedBuilder
            {
                Title = title,
                Color = new DiscordColor(2326507),
                Description = description
            }.Build();
        }

        public static DiscordEmbed EmbedAdminMessagesComposition(string description)
        {
            return new DiscordEmbedBuilder
            {
                Title = "Admin",
                Color = new DiscordColor(019300),
                Description = description
            }.Build();
        }
    }
}
