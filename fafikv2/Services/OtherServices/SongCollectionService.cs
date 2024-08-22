using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;
using Fafikv2.Data.DifferentClasses;
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
            var genresArray = await _spotifyApiService.GetGenresOfTrack(ctx.Message.Content.Remove(0, 6))
                .ConfigureAwait(false);
            var genres = string.Join(", ", genresArray);
            var songId = Guid.NewGuid();
            var song = new Song
            {
                Id = songId,
                Title = track.Title,
                Artist = track.Author,
                Genres = genres,
                LinkUri = track.Uri

            };

            var song1 = song;
            var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                var wasAddedBefore = await _songsService.Add(song1).ConfigureAwait(false);
                return wasAddedBefore;
            }).ConfigureAwait(false);

            if (result)
            {
                var song2 = song;
                song = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
                {
                    var result1 = await _songsService.Get(song2.Title, song2.Artist).ConfigureAwait(false);
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

            await _spotifyApiService
                .GetRecommendationsBasedOnInput(ctx.Message.Content.Remove(0, 6))
                .ConfigureAwait(false);
        }

        public async Task<LavalinkTrack> AutoPlay(LavalinkGuildConnection node, LavalinkTrack track)
        {

            var random = new Random();
            var spotifyRecommendationOrDatabase = random.Next(0, 3);


            /*if (spotifyRecommendationOrDatabase < 2) //do przebudowy, lepiej to będzie działać, jeżeli będzie pobierać piosenki w tym samym typie, a nie losowo
            {
                return await AutoPlayFromDatabaseSongs(node).ConfigureAwait(false);
            }*/

            return await AutoPlayFromSpotifyRecommendation(node, track).ConfigureAwait(false);



        }
        public async Task<LavalinkTrack> AutoPlayByGenre(LavalinkGuildConnection node, string genre)
        {
            var random = new Random();
            var spotifyRecommendationOrDatabase = random.Next(0, 6);

            return spotifyRecommendationOrDatabase switch
            {
                < 2 => await AutoPlayFromDatabaseSongsByGenre(node, genre).ConfigureAwait(false),
                >= 2 and < 4 => await AutoPlayFromSpotifyRecommendationByGenre(node, genre).ConfigureAwait(false),
                >= 4 => await AutoPlayFromDatabaseSongsOnlyByGenre(node, genre).ConfigureAwait(false)
            };
        }

        private async Task<LavalinkTrack> AutoPlayFromSpotifyRecommendation(LavalinkGuildConnection node,
            LavalinkTrack track)
        {
            var nextSongName =
                await _spotifyApiService.GetRecommendationsBasedOnInput(track.Title).ConfigureAwait(false);

            var nextSongResult =
                await node.GetTracksAsync(nextSongName, LavalinkSearchType.SoundCloud).ConfigureAwait(false);

            return nextSongResult.Tracks.First();
        }
        private async Task<LavalinkTrack> AutoPlayFromDatabaseSongs(LavalinkGuildConnection node)
        {
            var voiceChannel = node.Channel;
            var membersInChannel = voiceChannel.Users;
            List<Song> songs = new();

            var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {

                foreach (var user in membersInChannel)
                {
                    if (user.IsBot) continue;

                    var temporary = await _songsService.GetSongsByUser(Guid.Parse($"{user.Id:X32}"))
                        .ConfigureAwait(false);
                    temporary = temporary.Randomize(5);
                    songs.AddRange(temporary);


                }

                return true;
            }).ConfigureAwait(false);

            if (result)
            {
                Console.WriteLine("SongsFound");
            }

            var nextTrack = songs.Randomize(1).First();
            Console.WriteLine(nextTrack.Title);
            var nextSongResult = await node.GetTracksAsync(nextTrack.LinkUri).ConfigureAwait(false);

            return nextSongResult.Tracks.First();
        }

        private async Task<LavalinkTrack> AutoPlayFromSpotifyRecommendationByGenre(LavalinkGuildConnection node,
            string genre)
        {
            var nextSongName =
                await _spotifyApiService.GetRecommendationBasenOnGenre(genre).ConfigureAwait(false);

            var nextSongResult =
                await node.GetTracksAsync(nextSongName, LavalinkSearchType.SoundCloud).ConfigureAwait(false);

            return nextSongResult.Tracks.First();
        }

        private async Task<LavalinkTrack> AutoPlayFromDatabaseSongsByGenre(LavalinkGuildConnection node,string genre)
        {
            var voiceChannel = node.Channel;
            var membersInChannel = voiceChannel.Users;
            List<Song> songs = new();

            var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {

                foreach (var user in membersInChannel)
                {
                    if (user.IsBot) continue;

                    var temporary = await _songsService.GetSongsByGenreAndUser(genre,Guid.Parse($"{user.Id:X32}"))
                        .ConfigureAwait(false);
                    temporary = temporary.Randomize(5);
                    songs.AddRange(temporary);


                }

                return true;
            }).ConfigureAwait(false);

            if (result)
            {
                Console.WriteLine("SongsFound");
            }

            var nextTrack = songs.Randomize(1).First();
            Console.WriteLine(nextTrack.Title);
            var nextSongResult = await node.GetTracksAsync(nextTrack.LinkUri).ConfigureAwait(false);

            return nextSongResult.Tracks.First();
        }

        private async Task<LavalinkTrack> AutoPlayFromDatabaseSongsOnlyByGenre(LavalinkGuildConnection node, string genre)
        {
            
            List<Song> songs = new();

            var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                var temporary = await _songsService.GetSongByGenre(genre)
                    .ConfigureAwait(false);
                temporary = temporary.Randomize(1);
                songs.AddRange(temporary);

                return true;
            }).ConfigureAwait(false);

            if (result)
            {
                Console.WriteLine("SongsFound");
            }

            var nextTrack = songs.First();
            Console.WriteLine(nextTrack.Title);
            var nextSongResult = await node.GetTracksAsync(nextTrack.LinkUri).ConfigureAwait(false);

            return nextSongResult.Tracks.First();
        }

    }

}