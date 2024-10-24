﻿using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;
using Fafikv2.Data.DifferentClasses;
using Fafikv2.Data.Models;
using Fafikv2.Services.dbServices.Interfaces;
using Fafikv2.Services.OtherServices.Interfaces;
using Microsoft.IdentityModel.Tokens;

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
            var genresArray = await _spotifyApiService.GetGenresOfTrack(ctx.Message.Content.Remove(0, 6));
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
                var wasAddedBefore = await _songsService.Add(song1);
                return wasAddedBefore;
            });

            if (!result)
            {
                var song2 = song;
                song = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
                {
                    var result1 = await _songsService.Get(song2.Title, song2.Artist);
                    return result1;
                });
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
                await _userPlayedSongsService.Add(playedSong);
            });


            /* await _spotifyApiService
                 .GetRecommendationsBasedOnInput(ctx.Message.Content.Remove(0, 6))
                  ;*/

        }

        public async Task<LavalinkTrack?> AutoPlay(LavalinkGuildConnection node, LavalinkTrack track)
        {

            var random = new Random();
            var spotifyRecommendationOrDatabase = random.Next(0, 100);


            if (spotifyRecommendationOrDatabase < 40) //do przebudowy, lepiej to będzie działać, jeżeli będzie pobierać piosenki w tym samym typie, a nie losowo
            {
                return await AutoPlayFromDatabaseSongs(node, track);
            }

            return await AutoPlayFromSpotifyRecommendation(node, track);
        }
        public async Task<LavalinkTrack?> AutoPlayByGenre(LavalinkGuildConnection node, string? genre)
        {
            var random = new Random();
            var spotifyRecommendationOrDatabase = random.Next(0, 6);

            return spotifyRecommendationOrDatabase switch
            {
                < 2 => await AutoPlayFromDatabaseSongsByGenre(node, genre),
                >= 2 and < 4 => await AutoPlayFromSpotifyRecommendationByGenre(node, genre),
                >= 4 => await AutoPlayFromDatabaseSongsOnlyByGenre(node, genre)
            };
        }

        private async Task<LavalinkTrack?> AutoPlayFromSpotifyRecommendation(LavalinkGuildConnection node,
            LavalinkTrack track)
        {
            var searchQuery =
                await _spotifyApiService.GetRecommendationsBasedOnInput(track.Title);

            LavalinkTrack? recommendedTrack = new LavalinkTrack();

            if (searchQuery.IsNullOrEmpty())
            {
                recommendedTrack = await AutoPlayFromDatabaseSongs(node, track);
                return recommendedTrack;
            }

            //LavalinkTrack recommendedTrack = new LavalinkTrack();

            int i = 0;
            foreach (var nextSong in searchQuery)
            {
                var nextSongResult = await node.GetTracksAsync(nextSong, LavalinkSearchType.SoundCloud);

                recommendedTrack = nextSongResult.Tracks.FirstOrDefault();
                if (recommendedTrack != null) break;
                i++;



            }
            Console.WriteLine("Wpadło za " + i + " razem");
            //var recommendedTrack = nextSongResult.Tracks.FirstOrDefault();

            return recommendedTrack;
        }
        private async Task<LavalinkTrack?> AutoPlayFromDatabaseSongs(LavalinkGuildConnection node, LavalinkTrack track)
        {
            var voiceChannel = node.Channel;
            var membersInChannel = voiceChannel.Users;
            List<Song> songs = new();

            var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                string?[] genresResult = _spotifyApiService.GetGenresOfTrack(track.Title).Result;

                string genre = string.Join(", ", genresResult);

                foreach (var user in membersInChannel)
                {
                    if (user.IsBot) continue;
                    IEnumerable<Song> temporary;
                    if (genre.Length == 0)
                        temporary = await _songsService.GetSongsByUser(Guid.Parse($"{user.Id:X32}"));
                    else
                    {
                        temporary = await _songsService.GetSongsByGenreAndUser(genre, Guid.Parse($"{user.Id:X32}"));
                    }
                    temporary = temporary.Randomize(5);
                    var enumerable = temporary.ToList();
                    if (!enumerable.IsNullOrEmpty())
                        songs.AddRange(enumerable);


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

                        if (node.GetTracksAsync(temporary.LinkUri) == null) continue;
                        songs.Add(temporary);
                        break;

                    }
                }

                return true;
            });

            if (result)
            {
                Console.WriteLine("SongsFound");
            }

            var nextTrack = songs.Randomize(1).FirstOrDefault();
            Console.WriteLine(nextTrack?.Title);
            var nextSongResult = await node.GetTracksAsync(nextTrack?.LinkUri);

            return nextSongResult.Tracks.FirstOrDefault();
        }

        private async Task<LavalinkTrack?> AutoPlayFromSpotifyRecommendationByGenre(LavalinkGuildConnection node,
            string? genre)
        {
            var searchQuery =
                await _spotifyApiService.GetRecommendationBasenOnGenre(genre);


            LavalinkTrack? recommendedTrack = new LavalinkTrack();


            if (searchQuery.IsNullOrEmpty())
            {
                recommendedTrack = await AutoPlayFromDatabaseSongsOnlyByGenre(node, genre);
                return recommendedTrack;
            }

            foreach (var nextSong in searchQuery)
            {
                var nextSongResult = await node.GetTracksAsync(nextSong, LavalinkSearchType.SoundCloud);

                recommendedTrack = nextSongResult.Tracks.FirstOrDefault();

                if (recommendedTrack != null) break;

            }

            //var recommendedTrack = nextSongResult.Tracks.FirstOrDefault();
            if (recommendedTrack == null)
            {
                // Obsłuż sytuację, gdy nie znaleziono żadnych rekomendacji
                throw new InvalidOperationException("Brak rekomendacji z Spotify.");
            }

            return recommendedTrack;
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
                    temporary = temporary.Randomize(5);
                    songs.AddRange(temporary);


                }

                return true;
            });

            if (result)
            {
                Console.WriteLine("SongsFound");
            }

            if (songs.Count == 0)
            {
                return await AutoPlayFromSpotifyRecommendationByGenre(node, genre);

            }

            var nextTrack = songs.Randomize(1).First();
            Console.WriteLine(nextTrack.Title);
            var nextSongResult = await node.GetTracksAsync(nextTrack.LinkUri);

            return nextSongResult.Tracks.First();
        }

        private async Task<LavalinkTrack?> AutoPlayFromDatabaseSongsOnlyByGenre(LavalinkGuildConnection node, string? genre)
        {

            List<Song> songs = new();

            var result = await _databaseContextQueueService.EnqueueDatabaseTask(async () =>
            {
                var temporary = await _songsService.GetSongByGenre(genre);
                temporary = temporary.Randomize(1);
                songs.AddRange(temporary);

                return true;
            });

            if (result)
            {
                Console.WriteLine("SongsFound");
            }

            if (songs.Count == 0)
            {
                return await AutoPlayFromSpotifyRecommendationByGenre(node, genre);

            }

            var nextTrack = songs.Randomize(1).First();
            Console.WriteLine(nextTrack.Title);
            var nextSongResult = await node.GetTracksAsync(nextTrack.LinkUri);

            return nextSongResult.Tracks.First();
        }

    }

}