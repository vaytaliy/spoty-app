using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Dtos.Hub
{
    public class ChangeRoomPropertiesReadDto
    {
        public const string PasswordChange = "password_change";
        public const string RoomAccessChange = "room_public_access";
        public const string RoomFriendsOnly = "room_friends_only";

        [Required]
        [JsonPropertyName("changeType")]
        public string ChangeType { get; set; }
        [JsonPropertyName("setIsPrivateRoom")]
        public bool SetIsPrivateRoom { get; set; }
        [JsonPropertyName("setPassword")]
        public string SetPassword { get; set; }
        [JsonPropertyName("setIsFriendsOnly")]
        public bool SetIsFriendsOnly { get; set; }
    }
}
