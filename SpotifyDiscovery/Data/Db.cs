using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SpotifyDiscovery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Data
{
    public class Db
    {
        public readonly MongoClient client;
        public readonly IMongoDatabase database;
        private readonly ILogger<Db> _logger;

        public readonly IMongoCollection<Account> Account;
        public readonly IMongoCollection<PlayedMusic> PlayedMusic;
        public readonly IMongoCollection<Room> Room;
        private readonly IConfiguration _c;


        public Db(IConfiguration configuration, ILogger<Db> logger)
        {
            _logger = logger;
            _c = configuration;
            var connString = $"{_c["SpotifyDiscoveryDatabaseSettings:DbURL"]}";
            _logger.LogInformation(connString);
            client = new MongoClient(connString);
            database = client.GetDatabase("SpotifyDiscovery");

            Account = database.GetCollection<Account>("AccountCollectionName");
            PlayedMusic = database.GetCollection<PlayedMusic>("PlayedMusic");
            Room = database.GetCollection<Room>("Room");

            CreatePlayedMusicListIndex();
        }

        public void CreatePlayedMusicListIndex()
        {
            var trackedSongsIndexModel = new CreateIndexModel<PlayedMusic>(
                Builders<PlayedMusic>.IndexKeys.Ascending(pm => pm.SongIdList),
                new CreateIndexOptions()
            );
            PlayedMusic.Indexes.CreateOneAsync(trackedSongsIndexModel);
        }
    }
}
