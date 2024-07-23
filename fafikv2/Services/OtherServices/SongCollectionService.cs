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
            var genresArray = await _spotifyApiService.GetGenresOfTrackAsync(ctx.Message.Content.Remove(0,6)).ConfigureAwait(false);
            var genres = string.Join(", ", genresArray);
            var songId = Guid.NewGuid();
            var song=new Song
            {
                Id=songId,
                Title = track.Title,
                Artist = track.Author,
                Genres = genres,
                LinkUri = track.Uri

            };

            var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
               var wasAddedBefore= await _songsService.Add(song).ConfigureAwait(false);
                return wasAddedBefore;
            }).ConfigureAwait(false);

            if (result)
            {
                song = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
                {
                    var result1= await _songsService.Get(song.Title, song.Artist).ConfigureAwait(false);
                    return result1;
                }).ConfigureAwait(false);
            }
           

            var playedSong = new UserPlayedSong
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse($"{ctx.Message.Author.Id:X32}"),
                SongId = songId,
                Song = song
                
            };

            await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                await _userPlayedSongsService.Add(playedSong).ConfigureAwait(false);
                
            }).ConfigureAwait(false);

            var test = await _spotifyApiService
                .GetRecommendationsBasedOnInputAsync(ctx.Message.Content.Remove(0, 6))
                .ConfigureAwait(false);
        }

        
    }
}
