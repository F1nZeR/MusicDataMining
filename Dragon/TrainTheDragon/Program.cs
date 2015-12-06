using System;
using Core.Database;
using MongoDB.Driver;

namespace DragonClassifier
{
    class Program
    {
        static void Main()
        {
            //These evidences are used as training data for the Dragon Classigfier
            var positiveReviews = new Evidence("Positive", "Repository\\Positive.Evidence.csv");
            var negativeReviews = new Evidence("Negative", "Repository\\Negative.Evidence.csv");

            var classifier = new Classifier(positiveReviews, negativeReviews);

            DoWork(classifier);
            Console.ReadKey();
        }

        private static async void DoWork(Classifier classifier)
        {
            var items = await DataManager.GetSongsWithLyrics();

            foreach (var song in items)
            {
                var scores = classifier.Classify(song.Lyrics, DragonHelper.DragonHelper.ExcludeList);
                var positive = scores["Positive"];
                var negative = scores["Negative"];

                var filter = Builders<DbEntry>.Filter.Eq(x => x.Id, song.Id);
                var updateQuery = Builders<DbEntry>.Update.Set(x => x.Positive, positive).Set(x => x.Negative, negative);
                var result = await DataManager.Collection.UpdateOneAsync(filter, updateQuery);
            }

            Console.Out.WriteLine("Done!");
        }
    }
}
