﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Core.Database
{
    public class DataManager
    {
        private static IMongoClient _client;
        private static IMongoDatabase _database;
        private static IMongoCollection<DbEntry> _collection;

        public static IMongoCollection<DbEntry> Collection
        {
            get
            {
                if (_collection != null) return _collection;

                var connectionString = File.ReadAllText("connectionString.txt");
                _client = new MongoClient(connectionString);
                _database = _client.GetDatabase("test");
                _collection = _database.GetCollection<DbEntry>("music");
                return _collection;
            }
        } 

        public static async Task<IEnumerable<DbEntry>> GetAllSongs()
        {
            var items = Collection.Find(FilterDefinition<DbEntry>.Empty);
            return await items.ToListAsync();
        }

        public static async Task<IEnumerable<DbEntry>> GetSongsWithoutLyrics()
        {
            var filter = Builders<DbEntry>.Filter.Eq(x => x.Lyrics, null);
            var items = Collection.Find(filter);
            return await items.ToListAsync();
        }

        public static async Task<IEnumerable<DbEntry>> GetSongsWithLyrics()
        {
            var filter = Builders<DbEntry>.Filter.Ne(x => x.Lyrics, null);
            var items = Collection.Find(filter);
            return await items.ToListAsync();
        }
    }
}