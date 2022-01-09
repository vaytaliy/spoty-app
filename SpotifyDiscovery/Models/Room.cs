using Mapster;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Models
{
    [AdaptTo("[name]Dto"), GenerateMapper]
    public class Room
    {
        public const string IsFriendsOnlyBson = "isFriendsOnly";
        public const string AuthenticatedUsersBson = "authenticatedUserIds";
        public const string IsPrivateBson = "isPrivate";
        public const string PasswordBson = "password";
        public const string RoomIsActive = "isActive";
        public const string ActiveSongBson = "activeSong";

        public const int MaxPasswordLength = 8;

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("ownerId")]
        public string OwnerId { get; set; }
        [BsonElement("roomName")] //TODO: must be indexed, looks like so "Awesome Name#124", assigned number which doesn't exist in the room storage yet
        public string RoomName { get; set; }
        [BsonElement(IsFriendsOnlyBson)]
        public bool IsFriendsOnly { get; set; }
        [BsonElement(AuthenticatedUsersBson)]
        public BsonArray AuthenticatedUsers { get; set; }
        [BsonElement(IsPrivateBson)]
        public bool IsPrivate { get; set; }
        [BsonElement(PasswordBson)]
        [MaxLength(MaxPasswordLength)]
        public string Password { get; set; }
        [BsonElement(ActiveSongBson)]
        public string ActiveSong { get; set; }
        [BsonElement(RoomIsActive)]
        public bool IsActive { get; set; }
    }
}
