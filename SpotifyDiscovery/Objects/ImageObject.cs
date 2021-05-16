using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Objects
{
    public class ImageObject
    {
        [JsonPropertyName("height")]
        [BsonElement("height")]
        public int Height { get; set; }
        [JsonPropertyName("width")]
        [BsonElement("width")]
        public int Width { get; set; }
        [JsonPropertyName("url")]
        [BsonElement("url")]
        public string ImageURL { get; set; }
    }
}
