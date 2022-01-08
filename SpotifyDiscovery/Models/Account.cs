using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SpotifyDiscovery.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Models
{
    public class Account
    {
        public const string SpotifyIdBsonName = "id";
        public const string FriendlistBsonName = "spotifyId";

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement(SpotifyIdBsonName)]
        public string SpotifyId { get; set; }
        [BsonElement("nickname")]
        public string Nickname { get; set; }
        [BsonElement(FriendlistBsonName)]
        public BsonArray Friends { get; set; }
        [BsonElement("ProfileImages")]
        public List<ImageObject> ProfileImages { get; set; }
        [BsonElement("freshPlaylistId")]
        public string FreshPlaylistId { get; set; }
        [BsonElement("rooms")]
        public BsonArray Rooms { get; set; } //BSON array which keeps list of ObjectIds
    }
}
