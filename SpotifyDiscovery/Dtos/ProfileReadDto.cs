using SpotifyDiscovery.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Dtos
{
    public class ProfileReadDto
    {
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }
        [JsonPropertyName("id")]
        public string SpotifyId { get; set; }
        [JsonPropertyName("images")]
        public List<ImageObject> Images { get; set; }
    }
}
