using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;


namespace Fafikv2.Commands
{
    public class MusicPanel: BaseCommandModule
    {

        private readonly DiscordClient _client;

        // Konstruktor klasy MusicPanel przyjmuje instancję klienta Discorda
        public MusicPanel(DiscordClient client)
        {
            _client = client;

            // Rejestrujemy event obsługujący interakcje z przyciskami
            _client.ComponentInteractionCreated += async (_, e) =>
            {
                if (e.Id.StartsWith("option_"))
                {
                    // Obsługa kliknięcia przycisku
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent($"Wybrano: {e.Id}")
                            .AsEphemeral()); // Odpowiedź widoczna tylko dla osoby, która kliknęła
                }
            };
        }
        [Command("test")]
        public async Task SendPinnedMessageWithButtons(CommandContext ctx)
        {
            var messageBuilder = new DiscordMessageBuilder()
                .WithContent("Wybierz jedną z opcji poniżej:")
                .AddComponents(
                    new DiscordButtonComponent(ButtonStyle.Primary, "option_1", "Opcja 1"),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "option_2", "Opcja 2"),
                    new DiscordButtonComponent(ButtonStyle.Success, "option_3", "Opcja 3"),
                    new DiscordButtonComponent(ButtonStyle.Danger, "option_4", "Opcja 4")
                );
            
            // Wysyłanie wiadomości
            var message = await ctx.Channel.SendMessageAsync(messageBuilder);

            // Przypinanie wiadomości
            await message.PinAsync();

            // Event obsługujący interakcje z przyciskami
           
        }
    }
}
