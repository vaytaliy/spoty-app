using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Dtos.Hub
{
    public class ToggleFriendsOnlyResultDto
    {
        public const string SetResultSuccess = "set_new";
        public const string SetResultNoChange = "no_change";
        public const string ErrorResult = "error";

        [Required]
        [JsonPropertyName("result")]
        public string Result { get; set; }
        [JsonPropertyName("friendsOnlyAccess")]
        public bool FriendsOnlyAccess { get; set; }
        [JsonPropertyName("flushProfiles")]
        public List<string> FlushProfiles { get; set; }

        //public const string PasswordChange = "password_change";
        //public const string RoomPrivacy = "room_public_access";
        //public const string RoomIsFriendsOnly = "room_friends_only";

        //[Required]
        //[JsonPropertyName("changeType")]
        //public string ChangeType { get; set; }
        //[JsonPropertyName("payload")]
        //public object ChangePayload { get; set; }
    }
}
