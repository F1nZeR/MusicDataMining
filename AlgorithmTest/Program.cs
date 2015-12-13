using System;
using System.Data;
using System.Linq;
using Accord.MachineLearning;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.Math;
using Accord.Statistics.Distributions.DensityKernels;
using Core.Database;
using Accord.Statistics.Filters;
using AForge;
using Core.Models;

namespace AlgorithmTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //DoYourWork();
            Classification();
            Console.ReadKey();
        }

        private static void DoYourWork()
        {
            var songs = SongsFactory.GetSongsForLearning(); // этот метод вернёт песни, у которых есть ВСЕ данные

            var data = new DataTable("Songs Example");
            var columnNames =
                typeof (LearnModel).GetProperties()
                    .Select(p => p.Name)
                    .OrderBy(x => x)
                    .ToList();
            data.Columns.AddRange(columnNames.Select(name => new DataColumn(name)).ToArray());

            foreach (var song in songs)
            {
                data.Rows.Add(song.ArtistBeginYear, song.ArtistType, song.Duration, song.GenreType, song.LyricCharsCount,
                    song.LyricWordsCount, song.Negative, song.Popularity.ToString(), song.Positive, song.SongDateYear);
            }
            var codebook = new Codification(data, columnNames.ToArray());
            var symbols = codebook.Apply(data);
            var input = symbols.ToArray<double>(columnNames.Where(x => x != "Popularity").ToArray());
            var output = symbols.ToArray<int>("Popularity");

            var mins = new int[9];
            var maxs = new int[9];

            for (int i = 0; i < 9; i++)
            {
                var curMinForColumn = int.MaxValue;
                var curMaxForColumn = int.MinValue;
                for (int j = 0; j < input.GetLength(0); j++)
                {
                    var curValue = (int) input[j][i];
                    if (curValue < curMinForColumn) curMinForColumn = curValue;
                    if (curValue > curMaxForColumn) curMaxForColumn = curValue;
                }

                mins[i] = curMinForColumn;
                maxs[i] = curMaxForColumn;
            }

            DecisionVariable[] attributes =
            {
                new DecisionVariable("ArtistBeginYear", new IntRange(mins[0], maxs[0])),
                new DecisionVariable("ArtistType", songs.Select(x => x.ArtistType).Distinct().Count()),
                new DecisionVariable("Duration", new IntRange(mins[2], maxs[2])),
                new DecisionVariable("GenreType", songs.Select(x => x.GenreType).Distinct().Count()),
                new DecisionVariable("LyricCharsCount", new IntRange(mins[4], maxs[4])),
                new DecisionVariable("LyricWordsCount", new IntRange(mins[5], maxs[5])),               
                new DecisionVariable("Negative", new DoubleRange(songs.Min(x => x.Negative), songs.Max(x => x.Negative))),
                new DecisionVariable("Positive", new DoubleRange(songs.Min(x => x.Positive), songs.Max(x => x.Positive))),
                new DecisionVariable("SongDateYear", new DoubleRange(songs.Min(x => x.SongDateYear), songs.Max(x => x.SongDateYear))),
            };
            
            var classCount = 2; // popular, unpopular

            var tree = new DecisionTree(attributes, classCount);
            var algo = new C45Learning(tree);
            algo.Run(input, output);

            // проверяем своими данными
            data.Rows.Add(1966, 1, 302, 1, 1354, 255, 92.944470512297059, 0/*vashe pofig*/, 7.05552948770294, 2009);
            var lastItem = data.Rows[data.Rows.Count - 1];
            var input0 = codebook.Translate(lastItem, columnNames.Where(x => x != "Popularity").ToArray());
            var answer = tree.Compute(input0);
            var readableAnswer = codebook.Translate("Popularity", answer);
        }

        public static void Classification()
        {
            var songs = SongsFactory.GetSongsForLearning();

            double[][] observations = new double[songs.Count][];
            for (int i = 0; i < songs.Count; i++)
            {
                var song = songs[i];
                var items = new double[]
                {
                    song.ArtistBeginYear,
                    song.ArtistType,
                    song.Duration,
                    song.GenreType,
                    song.LyricCharsCount, song.LyricWordsCount,
                    song.Negative, song.Positive,
                    song.SongDateYear
                };
                observations[i] = items;
            }

            var kmeans = new KMeans(2);
            var labels = kmeans.Compute(observations);

            for (int i = 0; i < 2; i++)
            {
                var i1 = i;
                Console.Out.Write($"{labels.Count(x => x == i1)} ");
            }
            Console.Out.WriteLine();

            var correctCount = 0;
            for (int i = 0; i < songs.Count; i++)
            {
                if ((int) songs[i].Popularity == labels[i]) correctCount++;
            }

            Console.Out.WriteLine(correctCount);
        }
    }
}
