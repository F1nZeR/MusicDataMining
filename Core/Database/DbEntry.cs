using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace Core.Database
{
    public class DbEntry
    {
        public ObjectId Id { get; set; }

        /// <summary>
        /// Исполнитель
        /// </summary>
        public string ArtistName { get; set; }

        /// <summary>
        /// Название песни
        /// </summary>
        public string SongName { get; set; }

        /// <summary>
        /// Длительность в секундах
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Жанры
        /// </summary>
        public List<string> Genres { get; set; }

        /// <summary>
        /// Количество прослушиваний
        /// </summary>
        public int PlayCount { get; set; }

        /// <summary>
        /// Текст песни
        /// </summary>
        public string Lyrics { get; set; }

        /// <summary>
        /// Эмоции: положительное
        /// </summary>
        public double Positive { get; set; }

        /// <summary>
        /// Эмоции: отрицательное
        /// </summary>
        public double Negative { get; set; }

        /// <summary>
        /// Дата релиза песни
        /// </summary>
        public DateTime? SongDate { get; set; }

        /// <summary>
        /// Год создания коллектива
        /// </summary>
        public DateTime? ArtistBeginYear { get; set; }

        /// <summary>
        /// Тип артиста (группа, соло или ещё что)
        /// </summary>
        public string ArtistType { get; set; }

        public int? LyricCharsCount { get; set; }
        public int? LyricWordsCount { get; set; }
    }
}