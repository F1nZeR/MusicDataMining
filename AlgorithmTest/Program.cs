using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Database;

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

            foreach (var song in songs)
            {
                Console.Out.WriteLine($"{song.ArtistName} - {song.SongName}");
            }
        }
    }
}
