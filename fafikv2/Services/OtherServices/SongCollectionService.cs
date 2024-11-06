using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;
using Fafikv2.Data.DifferentClasses;
using Fafikv2.Data.Models;
using Fafikv2.Services.dbServices.Interfaces;
using Fafikv2.Services.OtherServices.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Serilog;
namespace Fafikv2.Services.OtherServices
{
    public class SongCollectionService : ISongCollectionService
    {
        private readonly ISongsService _songsService;
        private readonly IUserPlayedSongsService _userPlayedSongsService;
        private readonly ISpotifyApiService _spotifyApiService;
        private readonly IDatabaseContextQueueService _databaseContextQueueService;

        public SongCollectionService(ISongsService songsService, IUserPlayedSongsService userPlayedSongsService,
            ISpotifyApiService spotifyApiService, IDatabaseContextQueueService databaseContextQueueService)
        {
            _songsService = songsService;
            _userPlayedSongsService = userPlayedSongsService;
            _spotifyApiService = spotifyApiService;
            _databaseContextQueueService = databaseContextQueueService;
        }

        public async Task AddToBase(LavalinkTrack track, CommandContext ctx)
        {
            Log.Information("Adding track '{TrackTitle}' by '{Artist}' to the database.", track.Title, track.Author);
            var songId = Guid.NewGuid();
            var song = new Song
            {
                Id = songId,
                Title = track.Title,
                Artist = track.Author,
                LinkUri = track.Uri

            };

            var wasAddedBefore = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
                await _songsService.Add(song, _spotifyApiService.GetGenresOfTrack(ctx.Message.Content.Remove(0, 6))));

            if (!wasAddedBefore)
            {
                Log.Information("Track already exists. Retrieving existing song '{TrackTitle}' by '{Artist}'", song.Title, song.Artist);
                song = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
                    await _songsService.Get(song.Title, song.Artist)) ?? song; 
            }


            var playedSong = new UserPlayedSong
            {
                Id = Guid.NewGuid(),
                UserId = ctx.Message.Author.Id.ToGuid(),
                SongId = songId,
                Song = song

            };

            await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                await _userPlayedSongsService.Add(playedSong);
                Log.Information("User '{UserId}' played song '{TrackTitle}'.", ctx.Message.Author.Username, song.Title);
            });

        }

        public async Task<LavalinkTrack?> AutoPlay(LavalinkGuildConnection node, LavalinkTrack track) =>
            await AutoPlayBasedOnProbability(
                () => AutoPlayFromDatabaseSongs(node, track),
                () => AutoPlayFromSpotifyRecommendation(node, track),
                40);

        public async Task<LavalinkTrack?> AutoPlayByGenre(LavalinkGuildConnection node, string? genre) =>
            await AutoPlayBasedOnProbability(
                () => AutoPlayFromDatabaseSongsByGenre(node, genre),
                () => AutoPlayFromSpotifyRecommendationByGenre(node, genre),
                30);

        private static async Task<LavalinkTrack?> AutoPlayBasedOnProbability(
            Func<Task<LavalinkTrack?>> primarySource,
            Func<Task<LavalinkTrack?>> fallbackSource,
            int probability)
        {
            return new Random().Next(0, 100) < probability ? await primarySource() : await fallbackSource();
        }

        private async Task<LavalinkTrack?> AutoPlayFromSpotifyRecommendation(LavalinkGuildConnection node, LavalinkTrack track)
        {
            Log.Information("Fetching Spotify recommendations for track '{TrackTitle}'.", track.Title);


            var searchQuery = await _spotifyApiService.GetRecommendationsBasedOnInput(track.Title);
            if (searchQuery.IsNullOrEmpty())
            {
                Log.Warning("No Spotify recommendations found for track '{TrackTitle}'. Falling back to database songs.", track.Title);
                return await AutoPlayFromDatabaseSongs(node, track);
            }

            return await TryGetTrackFromSearchResults(node, searchQuery);
        }
        private async Task<LavalinkTrack?> AutoPlayFromDatabaseSongs(LavalinkGuildConnection node, LavalinkTrack track)
        {

            Log.Information("Autoplay from database songs for track '{TrackTitle}'", track.Title);

            var genres = await _spotifyApiService.GetGenresOfTrack(track.Title);
            var genre = string.Join(", ", genres);
            var songs = await GetUserSongsAsync(node.Channel.Users.Select(u => u.Id), genre);


            if (!songs.Any())
            {
                var genreSongs=await _databaseContextQueueService.EnqueueDatabaseTask(async()=> await _songsService.GetSongByGenre(genre));
                songs.AddRange(genreSongs.Randomize(2));
            }

            if (!songs.Any())
            {
                var randomSong = await _databaseContextQueueService.EnqueueDatabaseTask(async () => await _songsService.GetRandomSong());
                if (randomSong != null) songs.Add(randomSong);
            }

            var nextTrack = songs.Randomize(1).FirstOrDefault();
            return await TryGetTrackAsync(node, nextTrack?.LinkUri?.ToString());
        }

        private async Task<LavalinkTrack?> AutoPlayFromSpotifyRecommendationByGenre(LavalinkGuildConnection node, string? genre)
        {
            Log.Information("Fetching Spotify recommendations for genre '{Genre}'.", genre);

            var searchQuery = await _spotifyApiService.GetRecommendationBasenOnGenre(genre);
            if (searchQuery.IsNullOrEmpty())
            {
                Log.Warning("No Spotify recommendations found for genre. Falling back to database songs.");
                return await AutoPlayFromDatabaseSongsOnlyByGenre(node, genre);
            }

            return await TryGetTrackFromSearchResults(node, searchQuery);
        }

        private async Task<LavalinkTrack?> AutoPlayFromDatabaseSongsByGenre(LavalinkGuildConnection node, string? genre)
        {
            var songs = await GetUserSongsAsync(node.Channel.Users.Select(u => u.Id), genre);
            return songs.Any()
                ? await TryGetTrackAsync(node, songs.Randomize(1).FirstOrDefault()?.LinkUri?.ToString())
                : await AutoPlayFromSpotifyRecommendationByGenre(node, genre);
        }

        private async Task<LavalinkTrack?> AutoPlayFromDatabaseSongsOnlyByGenre(LavalinkGuildConnection node, string? genre)
        {
            Log.Information("Autoplay from database songs only by genre '{Genre}'", genre);

            var songs = (await _databaseContextQueueService.EnqueueDatabaseTask(async () => await _songsService.GetSongByGenre(genre))).Randomize(1).ToList();
            return songs.Any()
                ? await TryGetTrackAsync(node, songs.First().LinkUri?.ToString())
                : await AutoPlayFromSpotifyRecommendationByGenre(node, genre);
        }

        private static async Task<LavalinkTrack?> TryGetTrackFromSearchResults(LavalinkGuildConnection node, IEnumerable<string> searchResults)
        {
            foreach (var nextSong in searchResults)
            {
                var nextSongResult = await node.GetTracksAsync(nextSong, LavalinkSearchType.SoundCloud);
                var recommendedTrack = nextSongResult.Tracks.FirstOrDefault();
                if (recommendedTrack != null)
                {
                    Log.Information("Recommended track found: '{TrackTitle}' by '{Artist}'.", recommendedTrack.Title, recommendedTrack.Author);
                    return recommendedTrack;
                }
            }

            Log.Warning("No valid recommended tracks found.");
            return null;
        }
        private static async Task<LavalinkTrack?> TryGetTrackAsync(LavalinkGuildConnection node, string? uri)
        {
            try
            {
                var trackResult = await node.GetTracksAsync(uri);
                return trackResult.Tracks.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving track for URI: {Uri}", uri);
                return null;
            }
        }
        private async Task<List<Song>> GetUserSongsAsync(IEnumerable<ulong> userIds, string? genre)
        {
            var tasks = userIds.Select(userId =>
                string.IsNullOrEmpty(genre)
                    ? _databaseContextQueueService.EnqueueDatabaseTask(async() => await _songsService.GetSongsByUser(userId.ToGuid()))
                    : _databaseContextQueueService.EnqueueDatabaseTask(async () => await _songsService.GetSongsByGenreAndUser(genre, userId.ToGuid())));

            return (await Task.WhenAll(tasks)).SelectMany(songs => songs).ToList();
        }
    }
}