using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Dtos
{
    public class ChatMessageReadDto
    {
        [JsonPropertyName("sender")]
        public string Sender { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("thisUser")]
        public bool UserIsSender { get; set; }
    }
}
