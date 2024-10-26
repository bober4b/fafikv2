using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Fafikv2.Services.CommandService;

namespace Fafikv2.Commands
{
    public class MusicPanel : BaseCommandModule
    {
        private readonly DiscordClient _client;
        private readonly MusicService _musicService;

        // Constructor for the Music Panel class
        public MusicPanel(DiscordClient client, MusicService service)
        {
            _client = client;
            _musicService = service;

            // Registering the event to handle button interactions
            _client.ComponentInteractionCreated += async (_, e) =>
            {
                switch (e.Id)
                {
                    case "Play":
                        await HandlePlayButton(e);
                        break;
                    case "Skip":
                        await HandleSkipButton(e);
                        break;
                    case "Pause":
                        await HandlePauseButton(e);
                        break;
                    case "Auto_Play":
                        await HandleAutoPlayButton(e);
                        break;
                    default:
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder()
                                .WithContent("Unknown button clicked.")
                                .AsEphemeral());
                        break;
                }
            };
        }

        private async Task HandleAutoPlayButton(ComponentInteractionCreateEventArgs e)
        {
            await _musicService.AutoPlayFromPanel(_client, e);
        }

        private async Task HandlePauseButton(ComponentInteractionCreateEventArgs e)
        {
            await _musicService.PauseFromPanel(_client, e);
        }

        private async Task HandleSkipButton(ComponentInteractionCreateEventArgs e)
        {
            if(!await _musicService.SkipFromPanel(_client, e))
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            //await _musicService.SkipFromPanel(_client, e);

        }

        private async Task HandlePlayButton(ComponentInteractionCreateEventArgs e)
        {
            await _musicService.ResumeAsyncFromPanel(_client, e);

        }
    

        [Command("Panel")]
        public async Task SendPinnedMessageWithButtons(CommandContext ctx)
        {
            var x = await _musicService.GetMusicDictionary(ctx);
            var embed = new DiscordEmbedBuilder
            {
                Title = "NOW PLAYING: " + (x.Queue?.Count > 0 ? x.Queue[0].Title : "No track playing"),
                Color = new DiscordColor(2326507)
            }.Build();

            var messageBuilder = new DiscordMessageBuilder()
                .WithEmbed(embed)
                .AddComponents(
                    new DiscordButtonComponent(ButtonStyle.Primary, "Play", "Play", false,
                        new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶️"))),
                    new DiscordButtonComponent(ButtonStyle.Primary, "Skip", "Skip", false,
                        new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏭️"))),
                    new DiscordButtonComponent(ButtonStyle.Primary, "Pause", "Pause", false,
                        new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏸️")))

                ).AddComponents(
                    new DiscordButtonComponent(ButtonStyle.Primary, "Auto_Play", "Auto Play", false,
                        new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔀"))));

            // Sending the message
            /*var message = */await ctx.Channel.SendMessageAsync(messageBuilder);

            // Pinning the message
            //await message.PinAsync();

            
        }

    }
}
