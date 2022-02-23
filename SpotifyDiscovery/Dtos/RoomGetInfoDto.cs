using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Dtos
{
    public class RoomGetInfoDto
    {
        [JsonPropertyName("ownerId")]
        public string OwnerId { get; set; }
        [JsonPropertyName("activeSong")]
        public string ActiveSong { get; set; }
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
        [JsonPropertyName("requiresPassword")]
        public bool IsPasswordRequired { get; set; }
        [JsonPropertyName("isPrivate")]
        public bool IsPrivate { get; set; }
    }
}
