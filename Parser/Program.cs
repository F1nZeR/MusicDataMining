using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Database;
using Hqub.MusicBrainz.API.Entities;
using IF.Lastfm.Core.Objects;
using MongoDB.Driver;
using Parser.Helpers;

namespace Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            //PopulateDbWithSongsFromLastFm();
            //LoadLyricsForSongs();
            //TestWork();
            //LoadAdditionInfoAboutSongs();
            UpdateWordsCount();

            Console.ReadKey();
        }

        private static async void UpdateWordsCount()
        {

            var songs = await DataManager.GetSongsWithLyrics();
            foreach (var song in songs)
            {
                var filter = Builders<DbEntry>.Filter.Eq(x => x.Id, song.Id);
                var updateQuery = Builders<DbEntry>.Update.Set(x => x.LyricCharsCount, song.Lyrics.Length)
                    .Set(x => x.LyricWordsCount, StringUtilities.GetWords(song.Lyrics).Length);
                var result = await DataManager.Collection.UpdateOneAsync(filter, updateQuery);
            }
            Console.Out.WriteLine("Done!");
        }

        private static async void LoadAdditionInfoAboutSongs()
        {
            var songs = await DataManager.GetSongsWithLyrics();
            foreach (var song in songs)
            {
                var response = await LastFm.Client.Track.GetInfoAsync(song.SongName, song.ArtistName);
                if (!response.Success || response.Content.AlbumName == null) continue;

                Artist artist = null;
                while (artist == null)
                {
                    try
                    {
                        artist = await Artist.GetAsync(response.Content.ArtistMbid);
                    }
                    catch (Exception ex)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                }

                await Task.Delay(500);
                var lastAlbum = await LastFm.Client.Album.GetInfoAsync(song.ArtistName, response.Content.AlbumName);
                Release album = null;
                while (album == null)
                {
                    try
                    {
                        var release = await Release.GetAsync(lastAlbum.Content.Mbid);
                        if (release == null) break;
                        album = release;
                    }
                    catch (Exception ex)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                }

                DateTime? songDate = null;
                if (album?.Date != null)
                {
                    int year;
                    if (int.TryParse(album.Date, out year))
                    {
                        songDate = new DateTime(year, 1, 1);
                    }
                    else
                    {
                        try
                        {
                            songDate = DateTime.Parse(album.Date);
                        }
                        catch (Exception ex)
                        {
                            // ignored
                        }
                    }
                }

                DateTime? artistBeginDate = null;
                if (artist.LifeSpan?.Begin != null)
                {
                    int year;
                    if (int.TryParse(artist.LifeSpan.Begin, out year))
                    {
                        artistBeginDate = new DateTime(year, 1, 1);
                    }
                    else
                    {
                        try
                        {
                            artistBeginDate = DateTime.Parse(artist.LifeSpan.Begin);
                        }
                        catch (Exception ex)
                        {
                            // ignored
                        }
                    }
                }

                var filter = Builders<DbEntry>.Filter.Eq(x => x.Id, song.Id);
                var updateQuery = Builders<DbEntry>.Update.Set(x => x.ArtistType, artist.ArtistType)
                    .Set(x => x.ArtistBeginYear, artistBeginDate).Set(x => x.SongDate, songDate);
                var result = await DataManager.Collection.UpdateOneAsync(filter, updateQuery);

                Console.Out.WriteLine(song.ArtistName + " - " + song.SongName);
                await Task.Delay(TimeSpan.FromSeconds(2));
            }

            Console.Out.WriteLine("Done!");
        }

        private static async void TestWork()
        {
            var artists = await Artist.SearchAsync("Daft Punk");
            var artist = artists.First();

            var query = $"aid=({artist.Id}) release=({"One More Time"})";
            var album = (await Release.SearchAsync(Uri.EscapeUriString(query), 10)).First();

            //var response = await LastFm.Client.Track.GetInfoAsync("One More Time", "Daft Punk");
            //var album = await LastFm.Client.Album.GetInfoAsync("Daft Punk", "Discovery");
            //var url = album.Content.Url;

            //var album = await LastFm.Client.Album.GetInfoAsync("Daft Punk", "Discovery");
            //var album2 = await LastFm.Client.Album.GetInfoByMbidAsync(album.Content.Mbid);
            Console.Out.WriteLine("Done!");
        }

        private static async void LoadLyricsForSongs()
        {
            var songs = await DataManager.GetSongsWithoutLyrics();
            foreach (var song in songs)
            {
                var lyric = LyricsEngine.PullLyrics(song.ArtistName, song.SongName);

                if (string.IsNullOrEmpty(lyric))
                {
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.Out.WriteLine("NOT FOUND:\t" + song.ArtistName + " - " + song.SongName);
                    Console.ResetColor();
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    continue;
                }

                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.Out.WriteLine(song.ArtistName + " - " + song.SongName);
                Console.ResetColor();

                var filter = Builders<DbEntry>.Filter.Eq(x => x.Id, song.Id);
                var updateQuery = Builders<DbEntry>.Update.Set(entry => entry.Lyrics, lyric);
                var result = await DataManager.Collection.UpdateOneAsync(filter, updateQuery);
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
            Console.Out.WriteLine("Done!");
        }

        public static async void PopulateDbWithSongsFromLastFm()
        {
            var curStep = 0;

            var response = await LastFm.Client.Chart.GetTopArtistsAsync(3, 40);

            var neededSongs = new List<LastTrack>();
            // исполнители
            var artists = response.Content;
            foreach (var artist in artists)
            {
                var songs = await LastFm.Client.Artist.GetTopTracksAsync(artist.Name, false, 1, 10);
                
                // песни с длительностью, жанром и кол-вом прослушиваний
                foreach (var neededSong in songs.Content)
                {
                    var song = await LastFm.Client.Track.GetInfoAsync(neededSong.Name, artist.Name);
                    neededSongs.Add(song.Content);
                    Console.Out.WriteLine($"{artist.Name} - {song.Content.Name}");
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                var tagQuery = await LastFm.Client.Artist.GetTopTagsAsync(artist.Name);
                var tags = tagQuery.Content.Select(x => x.Name).ToList();

                var resSongs = neededSongs.Where(x => x.Duration.HasValue && x.PlayCount.HasValue).ToList();
                if (resSongs.Count > 0)
                {
                    var items = resSongs.Select(x => new DbEntry
                    {
                        ArtistName = x.ArtistName,
                        SongName = x.Name,
                        Genres = tags,
                        Duration = (int)(x.Duration?.TotalSeconds ?? 0),
                        PlayCount = x.PlayCount ?? 0
                    });
                    await DataManager.Collection.InsertManyAsync(items);
                }

                neededSongs.Clear();
                Console.Out.WriteLine($"Step {++curStep} ({artist.Name}): {resSongs.Count} entries.");
                await Task.Delay(TimeSpan.FromSeconds(2));
            }

            Console.Out.WriteLine("Done!");
        }
    }
}
