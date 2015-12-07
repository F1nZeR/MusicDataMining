using System;
using System.Data;
using System.Linq;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.Math;
using Core.Database;
using Accord.Statistics.Filters;

namespace AlgorithmTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DoYourWork();
            Console.ReadKey();
        }

        private static void DoYourWork()
        {
            var songs = DataManager.GetSongsForDataMining(); // этот метод вернёт песни, у которых есть ВСЕ данные

            var data = new DataTable("Songs Example");
            var columnNames =
                typeof (DbEntry).GetProperties()
                    .Select(p => p.Name)
                    .OrderBy(x => x)
                    .Where(x => x != "Genres" && x != "Id")
                    .ToList();
            data.Columns.AddRange(columnNames.Select(name => new DataColumn(name)).ToArray());

            foreach (var song in songs)
            {
                data.Rows.Add(song.ArtistBeginYear, song.ArtistName, song.ArtistType, song.Duration, /*song.Genres,*/
                    song.LyricCharsCount, song.Lyrics, song.LyricWordsCount, song.Negative, song.PlayCount,
                    song.Positive, song.SongDate, song.SongName);
            }
            var codebook = new Codification(data, columnNames.Where(x => x != "PlayCount").ToArray());
            var symbols = codebook.Apply(data);
            var input = symbols.ToArray<int>(columnNames.Where(x => x != "PlayCount").ToArray());
            var output = symbols.ToArray<int>("PlayCount");

            DecisionVariable[] attributes =
            {
                new DecisionVariable("ArtistBeginYear", songs.GroupBy(x => x.ArtistBeginYear).Count()),
                new DecisionVariable("ArtistName", songs.GroupBy(x => x.ArtistName).Count()),
                new DecisionVariable("ArtistType", songs.GroupBy(x => x.ArtistType).Count()),
                new DecisionVariable("Duration", songs.GroupBy(x => x.Duration).Count()),
//                new DecisionVariable("Genres", songs.GroupBy(x => x.Genres).Count()),
                new DecisionVariable("LyricCharsCount", songs.GroupBy(x => x.LyricCharsCount).Count()),
                new DecisionVariable("Lyrics", songs.GroupBy(x => x.Lyrics).Count()),
                new DecisionVariable("LyricWordsCount", songs.GroupBy(x => x.LyricWordsCount).Count()),               
                new DecisionVariable("Negative", songs.GroupBy(x => x.Negative).Count()),
                new DecisionVariable("Positive", songs.GroupBy(x => x.Positive).Count()),
                new DecisionVariable("SongDate", songs.GroupBy(x => x.SongDate).Count()),
                new DecisionVariable("SongName", songs.GroupBy(x => x.SongName).Count()),
            };

            //По идее, кол-во классов == кол-ву разных выходов. А вот хуй. Ему нужно, чтоб classCount > любого значения из output
            //int classCount = songs.GroupBy(x => x.PlayCount).Count(); 

            int classCount = 0;
            for (int i = 0; i < output.Length; i++)
            {
                if (output[i] > classCount) classCount = output[i];
            }
            classCount++;

            var tree = new DecisionTree(attributes, classCount);
            var id3Learning = new ID3Learning(tree);
            id3Learning.Run(input, output);
            var answer = codebook.Translate("PlayCount", tree.Compute(
                codebook.Translate("31.12.1991 18:00:00", "Daft Punk", "Group", "207", "16", "Instumental", "1", "50", "50", "09.03.2001 18:00:00", "Aerodynamic")));


        }
    }
}
