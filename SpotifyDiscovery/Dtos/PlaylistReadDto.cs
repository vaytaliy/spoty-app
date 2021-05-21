using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Dtos
{
    public class PlaylistReadDto
    {
        [JsonPropertyName("id")]
        public string PlaylistId { get; set; }
    }
}
