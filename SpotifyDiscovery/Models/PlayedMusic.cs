using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SpotifyDiscovery.Models
{
    public class PlayedMusic //Contains identifiers of music that won't be played again
    {
        public const string AccountIdFromJson = "accountId";
        public const string SongListFromJson = "songs";

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)] //Mongodb intro https://www.mongodb.com/blog/post/quick-start-c-and-mongodb--read-operations
        public string Id { get; set; }
        [BsonElement(AccountIdFromJson)]
        public string AccountId { get; set; }
        [BsonElement(SongListFromJson)]
        public BsonArray SongIdList { get; set; } //db.spotify.createIndex({"SongIdList": 1}) //https://docs.mongodb.com/manual/core/index-multikey/

        //create multikey index for this array https://stackoverflow.com/questions/4059126/how-does-mongodb-index-arrays
    }   //https://stackoverflow.com/questions/22947857/mongodb-array-query-performance
}
