using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SpotifyDiscovery.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Dtos
{
    public class AccountReadDto
    {
        [JsonPropertyName("id")]
        public string SpotifyId { get; set; }
        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }
        [JsonPropertyName("friendList")]
        public BsonArray Friends { get; set; }
        [JsonPropertyName("profileImages")]
        public List<ImageObject> ProfileImages { get; set; }
        [JsonPropertyName("freshPlaylistId")]
        public string FreshPlaylistId { get; set; }
    }
}
