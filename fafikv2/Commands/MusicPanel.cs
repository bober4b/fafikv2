using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Fafikv2.Commands.MessageCreator;
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
                    case "Queue":
                        await HandleQueueButton(e);
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

        private async Task HandleQueueButton(ComponentInteractionCreateEventArgs e)
        {
           var result= await _musicService.QueueFromPanel(e);

           await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
               new DiscordInteractionResponseBuilder()
                   .AddEmbed(MessagesComposition.EmbedQueueComposition(result))
                   .AsEphemeral());
        }

        private async Task HandleAutoPlayButton(ComponentInteractionCreateEventArgs e)
        {
            await _musicService.AutoPlayFromPanel(_client, e);
        }

        private async Task HandlePauseButton(ComponentInteractionCreateEventArgs e)
        {
            await MusicService.PauseFromPanel(_client, e);
        }

        private async Task HandleSkipButton(ComponentInteractionCreateEventArgs e)
        {
            if (await _musicService.SkipFromPanel(_client, e))
            {
                
               await e.Message.ModifyAsync(
                   MessagesComposition.EmbedPanelComposition(
                       await _musicService.GetMusicDictionary(e.Guild)));
            }
            //await _musicService.SkipFromPanel(_client, e);


        }

        private async Task HandlePlayButton(ComponentInteractionCreateEventArgs e)
        {
            await MusicService.ResumeAsyncFromPanel(_client, e);

        }
        
        [Command("Panel")]
        public async Task SendPinnedMessageWithButtons(CommandContext ctx)
        {
            var x = await _musicService.GetMusicDictionary(ctx.Guild);
            
            await ctx.Channel.SendMessageAsync(MessagesComposition.EmbedPanelComposition(x));

        }
    }
}
