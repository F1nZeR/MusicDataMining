using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Database;
using IF.Lastfm.Core.Objects;
using MongoDB.Driver;

namespace Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            //PopulateDbWithSongsFromLastFm();
            //LoadLyricsForSongs();
            TestWork();

            Console.ReadKey();
        }

        private static async void TestWork()
        {
            var response = await LastFm.Client.Track.GetInfoAsync("Omen", "The Prodigy");
            var album = await LastFm.Client.Album.GetInfoAsync("The Prodigy", response.Content.AlbumName);
            var album2 = await LastFm.Client.Album.GetInfoByMbidAsync(album.Content.Mbid);
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
