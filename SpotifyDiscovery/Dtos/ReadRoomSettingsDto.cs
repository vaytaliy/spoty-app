using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Dtos
{
    public class ReadRoomSettingsDto
    {
        [JsonPropertyName("friends")]
        public List<object> Friends { get; set; }
        [JsonPropertyName("roomPassword")]
        public string RoomPassword { get; set; }
        [JsonPropertyName("maxConnections")]
        public int MaxConnections { get; set; }
        [JsonPropertyName("isFriendsOnly")]
        public bool IsFriendsOnly { get; set; }
    }
}
