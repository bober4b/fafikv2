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
            var genres = string.Join(", ", await _spotifyApiService.GetGenresOfTrack(ctx.Message.Content.Remove(0, 6)));
            var songId = Guid.NewGuid();
            var song = new Song
            {
                Id = songId,
                Title = track.Title,
                Artist = track.Author,
                Genres = genres,
                LinkUri = track.Uri

            };

            var wasAddedBefore = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
                await _songsService.Add(song));

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

        public async Task<LavalinkTrack?> AutoPlay(LavalinkGuildConnection node, LavalinkTrack track)
        {
            Log.Information("Attempting to autoplay track '{TrackTitle}'.", track.Title);
            return new Random().Next(0, 100) < 40
                ? await AutoPlayFromDatabaseSongs(node, track)
                : await AutoPlayFromSpotifyRecommendation(node, track);
        }
        public async Task<LavalinkTrack?> AutoPlayByGenre(LavalinkGuildConnection node, string? genre)
        {
            Log.Information("Attempting to autoplay by genre '{Genre}'.", genre);
            var random = new Random().Next(0, 6);

            return random switch
            {
                < 2 => await AutoPlayFromDatabaseSongsByGenre(node, genre),
                >= 2 and < 4 => await AutoPlayFromSpotifyRecommendationByGenre(node, genre),
                _ => await AutoPlayFromDatabaseSongsOnlyByGenre(node, genre)
            };
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

            foreach (var nextSong in searchQuery)
            {
                var nextSongResult = await node.GetTracksAsync(nextSong, LavalinkSearchType.SoundCloud);
                var recommendedTrack = nextSongResult.Tracks.FirstOrDefault();
                if (recommendedTrack != null)
                {
                    Log.Information("Recommended track found: '{TrackTitle}' by '{Artist}'.", recommendedTrack.Title, recommendedTrack.Author);
                    return recommendedTrack;
                }
            }

            Log.Warning("No valid recommended tracks found from Spotify.");
            return null; 
        }
        private async Task<LavalinkTrack?> AutoPlayFromDatabaseSongs(LavalinkGuildConnection node, LavalinkTrack track)
        {

            Log.Information("Autoplay from database songs for track '{TrackTitle}'", track.Title);

            var voiceChannel = node.Channel;
            var membersInChannel = voiceChannel.Users;
            var songs = new List<Song>();



            var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                string?[] genresResult = _spotifyApiService.GetGenresOfTrack(track.Title).Result;

                string genre = string.Join(", ", genresResult);

                foreach (var user in membersInChannel)
                {
                    if (user.IsBot) continue;
                    IEnumerable<Song> temporary;
                    if (string.IsNullOrEmpty(genre))
                        temporary = await _songsService.GetSongsByUser(Guid.Parse($"{user.Id:X32}"));
                    else
                    {
                        temporary = await _songsService.GetSongsByGenreAndUser(genre, Guid.Parse($"{user.Id:X32}"));
                    }
                    temporary = temporary.Randomize(5);
                    var enumerable = temporary.ToList();
                    if (enumerable.Any())
                    {
                        songs.AddRange(enumerable);
                    }


                }

                if (!string.IsNullOrEmpty(genre))
                {
                    songs.AddRange((await _songsService.GetSongByGenre(genre)).Randomize(2));
                }

                if (songs.Count == 0)
                {
                    while (true)
                    {
                        var temporary = await _songsService.GetRandomSong();
                        if (temporary == null) continue;

                        if (await node.GetTracksAsync(temporary.LinkUri) == null) continue;
                        songs.Add(temporary);
                        break;

                    }
                }

                return true;
            });

            if (result)
            {
                Log.Information("Songs found for autoplay from database");
            }

            var nextTrack = songs.Randomize(1).FirstOrDefault();

            Log.Information("Next track selected: '{TrackTitle}'", nextTrack?.Title);

            var nextSongResult = await node.GetTracksAsync(nextTrack?.LinkUri);

            return nextSongResult.Tracks.FirstOrDefault();
        }

        private async Task<LavalinkTrack?> AutoPlayFromSpotifyRecommendationByGenre(LavalinkGuildConnection node, string? genre)
        {
            Log.Information("Fetching Spotify recommendations for genre '{Genre}'.", genre);
            var searchQuery = await _spotifyApiService.GetRecommendationBasenOnGenre(genre);
            if (searchQuery.IsNullOrEmpty())
            {
                Log.Warning("No Spotify recommendations found for genre '{Genre}'. Falling back to database songs.", genre);
                return await AutoPlayFromDatabaseSongsOnlyByGenre(node, genre);
            }

            foreach (var nextSong in searchQuery)
            {
                var nextSongResult = await node.GetTracksAsync(nextSong, LavalinkSearchType.SoundCloud);
                var recommendedTrack = nextSongResult.Tracks.FirstOrDefault();
                if (recommendedTrack != null)
                {
                    Log.Information("Recommended track found for genre '{Genre}': '{TrackTitle}' by '{Artist}'.", genre, recommendedTrack.Title, recommendedTrack.Author);
                    return recommendedTrack;
                }
            }

            Log.Warning("No valid recommended tracks found for genre '{Genre}'.", genre);
            throw new InvalidOperationException("Brak rekomendacji z Spotify.");
        }

        private async Task<LavalinkTrack?> AutoPlayFromDatabaseSongsByGenre(LavalinkGuildConnection node, string? genre)
        {
            var voiceChannel = node.Channel;
            var membersInChannel = voiceChannel.Users;
            List<Song> songs = new();

            var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {

                foreach (var user in membersInChannel)
                {
                    if (user.IsBot) continue;

                    var temporary = await _songsService.GetSongsByGenreAndUser(genre, Guid.Parse($"{user.Id:X32}"));
                    if (!temporary.Any()) continue;
                    var randomizedSongs = temporary.Randomize(5);
                    songs.AddRange(randomizedSongs);
                    Log.Information("Found {Count} songs for user '{UserId}'", randomizedSongs.Count(), user.Id);


                }

                return true;
            });

            if (result)
            {
                Log.Information("Songs found for genre '{Genre}'", genre);
            }

            if (songs.Count == 0)
            {
                return await AutoPlayFromSpotifyRecommendationByGenre(node, genre);

            }

            var nextTrack = songs.Randomize(1).First();
            Log.Information("Next track selected: '{TrackTitle}'", nextTrack.Title);
            var nextSongResult = await node.GetTracksAsync(nextTrack.LinkUri);

            return nextSongResult.Tracks.FirstOrDefault();
        }

        private async Task<LavalinkTrack?> AutoPlayFromDatabaseSongsOnlyByGenre(LavalinkGuildConnection node, string? genre)
        {
            Log.Information("Autoplay from database songs only by genre '{Genre}'", genre);

            List<Song> songs = new();

            var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                var temporary = await _songsService.GetSongByGenre(genre);
                temporary = temporary.Randomize(1);
                if (temporary.Any())
                {
                    var randomizedSongs = temporary.Randomize(1);
                    songs.AddRange(randomizedSongs);
                    Log.Information("Found {Count} songs for genre '{Genre}'", randomizedSongs.Count(), genre);
                }

                return true;
            });

            if (result)
            {
                Log.Information("Songs found for genre '{Genre}'", genre);
            }

            if (songs.Count == 0)
            {
                Log.Information("No songs found, trying Spotify recommendations for genre '{Genre}'", genre);
                return await AutoPlayFromSpotifyRecommendationByGenre(node, genre);
            }

            var nextTrack = songs.Randomize(1).First();
            Log.Information("Next track selected: '{TrackTitle}'", nextTrack.Title);
            var nextSongResult = await node.GetTracksAsync(nextTrack.LinkUri);

            return nextSongResult.Tracks.FirstOrDefault();
        }

    }

}