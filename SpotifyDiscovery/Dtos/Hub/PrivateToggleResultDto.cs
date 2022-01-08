using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Dtos.Hub
{
    public class PrivateToggleResultDto
    {
        public const string SetResultSuccess = "set_new";
        public const string SetResultNoChange = "no_change";
        public const string ErrorResult = "error";

        [Required]
        [JsonPropertyName("result")]
        public string Result { get; set; }
        [JsonPropertyName("errorDescription")]
        public string ErrorDescription { get; set; }
        [JsonPropertyName("accessChange")]
        public bool AccessChange { get; set; }
    }
}
