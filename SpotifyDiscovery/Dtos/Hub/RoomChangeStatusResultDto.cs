using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Dtos.Hub
{
    public class RoomChangeStatusResultDto
    {
        public const string PasswordChange = "password_change";
        public const string RoomPrivacy = "room_public_access";
        public const string RoomIsFriendsOnly = "room_friends_only";

        [Required]
        [JsonPropertyName("changeType")]
        public string ChangeType { get; set; }
        [JsonPropertyName("payload")]
        public object ChangePayload { get; set; }
    }
}

