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

        public readonly IMongoCollection<Account> Account;
        public readonly IMongoCollection<PlayedMusic> PlayedMusic;
        public readonly IMongoCollection<Room> Room;


        public Db(ISpotifyDiscoveryDatabaseSettings settings)
        {
            client = new MongoClient(settings.ConnectionString);
            database = client.GetDatabase(settings.DatabaseName);

            Account = database.GetCollection<Account>(settings.AccountCollectionName);
            PlayedMusic = database.GetCollection<PlayedMusic>(settings.PlayedMusicCollectionName);
            Room = database.GetCollection<Room>(settings.RoomCollectionName);
        }
    }
}
