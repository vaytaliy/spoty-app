using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Models
{
    public class Account
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("spotifyId")]
        public string SpotifyId { get; set; }
        [BsonElement("nickname")]
        public string Nickname { get; set; }
        [BsonElement("friendList")]
        public BsonArray Friends { get; set; }
        [BsonElement("profilePictureURL")]
        [BsonDefaultValue("place default image url here")] //TODO: filesystem
        public string ProfilePictureURL { get; set; }
        [BsonElement("freshPlaylists")]
        public string FreshPlaylistsIdentifiers { get; set; }
        [BsonElement("rooms")]
        public BsonArray Rooms { get; set; } //BSON array which keeps list of ObjectIds
    }
}
