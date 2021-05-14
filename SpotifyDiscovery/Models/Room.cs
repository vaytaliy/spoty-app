using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Models
{
    public class Room
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("ownerId")]
        public string OwnerId { get; set; }
        [BsonElement("roomName")] //TODO: must be indexed, looks like so "Awesome Name#124", assigned number which doesn't exist in the room storage yet
        public string RoomName { get; set; }
        [BsonElement("roomPictureURL")]
        public string RoomPictureURL { get; set; }
        [BsonElement("backgroundPictureURL")]
        public string BackgroundPictureURL { get; set; }
        [BsonElement("musicList")]
        public BsonArray MusicList { get; set; }
        [BsonElement("isFriendsOnly")]
        public bool IsFriendsOnly { get; set; }
        [BsonElement("isPrivate")]
        public bool IsPrivate { get; set; }
        [BsonElement("isAllowedMembers")]
        public bool IsAllowedMembers { get; set; }
        [BsonElement("authorizedMembers")]
        public BsonArray AuthorizedMembers { get; set; }
        [BsonElement("password")]
        public string Password { get; set; }
    }
}
