using System;
using System.Collections.Generic;
using System.Linq;
using Core.Database;

namespace Core.Models
{
    public class SongsFactory
    {
        public static List<LearnModel> GetSongsForLearning()
        {
            //var rand = new Random();
            //var songs = DataManager.GetSongsForDataMining().OrderByDescending(x => rand.Next()).Take(350).OrderByDescending(x => x.PlayCount).ToList();
            var songs = DataManager.GetSongsForDataMining().OrderByDescending(x => x.PlayCount).ToList();
            var popularityMedian = songs.Count/2;

            var genres = songs.Select(x => x.Genres.First()).Distinct().ToList();

            var dictIndex = 0;
            var genresIdsDict = genres.ToDictionary(key => ++dictIndex, value => value);
            dictIndex = 0;
            var genresStrsDict = genres.ToDictionary(key => key, value => ++dictIndex);
            LearnModel.GenresDict = genresIdsDict;

            var artistTypes = songs.Select(x => x.ArtistType).Distinct().ToList();
            dictIndex = 0;
            var artistTypesStrs = artistTypes.ToDictionary(key => key, value => ++dictIndex);
            dictIndex = 0;
            var artistIdTypes = artistTypes.ToDictionary(key => ++dictIndex, value => value);
            LearnModel.ArtistTypesDict = artistIdTypes;

            var resList = new List<LearnModel>();
            var curIndex = 0;
            foreach (var song in songs)
            {
                var genreType = genresStrsDict[song.Genres.First()];
                var artistType = artistTypesStrs[song.ArtistType];

                var resSong = new LearnModel
                {
                    Duration = song.Duration,
                    ArtistBeginYear = song.ArtistBeginYear.Value.Year,
                    LyricCharsCount = song.LyricCharsCount.Value,
                    LyricWordsCount = song.LyricWordsCount.Value,
                    Negative = song.Negative,
                    Positive = song.Positive,
                    SongDateYear = song.SongDate.Value.Year,
                    GenreType = genreType,
                    ArtistType = artistType,
                    Popularity = curIndex < popularityMedian ? PopularityEnum.Popular : PopularityEnum.Unpopular
                };

                resList.Add(resSong);

                curIndex++;
            }

            return resList;
        }
    }
}