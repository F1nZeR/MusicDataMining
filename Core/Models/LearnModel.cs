using System;
using System.Collections.Generic;

namespace Core.Models
{
    public class LearnModel
    {
        public int LyricCharsCount { get; set; }
        public int LyricWordsCount { get; set; }
        public double Positive { get; set; }
        public double Negative { get; set; }
        public int Duration { get; set; }
        public int SongDateYear { get; set; }
        public int ArtistBeginYear { get; set; }
        public int GenreType { get; set; }
        public int ArtistType { get; set; }

        public PopularityEnum Popularity { get; set; }

        public string GetGenreAsString()
        {
            return GenresDict[GenreType];
        }

        public string GetArtistTypeAsString()
        {
            return ArtistTypesDict[ArtistType];
        }

        internal static Dictionary<int, string> GenresDict;
        internal static Dictionary<int, string> ArtistTypesDict;
    }
}