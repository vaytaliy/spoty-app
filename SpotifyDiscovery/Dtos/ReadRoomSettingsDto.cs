using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Dtos
{
    public class ReadRoomSettingsDto
    {
        [JsonPropertyName("passwordRequired")]
        public bool PasswordRequired { get; set; }
    }
}
