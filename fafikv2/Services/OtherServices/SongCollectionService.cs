using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;
using Fafikv2.Data.Models;
using Fafikv2.Services.dbServices.Interfaces;
using Fafikv2.Services.OtherServices.Interfaces;

namespace Fafikv2.Services.OtherServices
{
    public class SongCollectionService : ISongCollectionService
    {
        private readonly ISongsService _songsService;
        private readonly IUserPlayedSongsService _userPlayedSongsService;
        private readonly ISpotifyApiService _spotifyApiService;
        private readonly IDatabaseContextQueueService _databaseContextQueueService;
        public SongCollectionService(ISongsService songsService, IUserPlayedSongsService userPlayedSongsService, ISpotifyApiService spotifyApiService, IDatabaseContextQueueService databaseContextQueueService)
        {
            _songsService = songsService;
            _userPlayedSongsService = userPlayedSongsService;
            _spotifyApiService = spotifyApiService;
            _databaseContextQueueService = databaseContextQueueService;
        }

        public async Task AddToBase(LavalinkTrack track, CommandContext ctx)
        {
            var genresArray = await _spotifyApiService.GetGenresOfTrackAsync(track.Title, track.Author).ConfigureAwait(false);
            var genres = string.Join(", ", genresArray);
            var songId = Guid.NewGuid();
            var song=new Song
            {
                Id=songId,
                Title = track.Title,
                Artist = track.Author,
                Genres = genres

            };

            var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                await _songsService.Add(song).ConfigureAwait(false);
                return true;
            }).ConfigureAwait(false);


           

            var playedSong = new UserPlayedSong
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse($"{ctx.Message.Author.Id:X32}"),
                SongId = songId,
                
            };

            var res=await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                await _userPlayedSongsService.Add(playedSong).ConfigureAwait(false);
                return true;
            }).ConfigureAwait(false);
        }
    }
}
