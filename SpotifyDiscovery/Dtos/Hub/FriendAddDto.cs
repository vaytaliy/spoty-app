using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Dtos.Hub
{
    public class FriendAddDto
    {
        [Required]
        [RegularExpression("added|removed|error")]
        [JsonPropertyName("result")]
        public string Result { get; set; }
        [Required]
        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
