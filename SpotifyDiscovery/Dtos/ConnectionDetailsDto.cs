using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Dtos
{
    public class ConnectionDetailsDto
    {
        [JsonPropertyName("room_id")]
        public string RoomId { get; set; }
        [JsonPropertyName("connection_detail")]
        public List<ProfileReadDto> ConnectionDetail { get; set; }
    }
}
