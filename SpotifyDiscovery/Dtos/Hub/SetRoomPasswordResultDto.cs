using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Dtos.Hub
{
    public class SetRoomPasswordResultDto
    {
        public const string SetResultSuccess = "set_new";
        public const string SetResultNoChange = "no_change";
        public const string ErrorResult = "error";

        [Required]
        [JsonPropertyName("result")]
        public string Result { get; set; }
        [JsonPropertyName("newPassword")]
        public string NewPassword { get; set; }
        [JsonPropertyName("error")]
        public string Error { get; set; }
        [JsonPropertyName("description")]
        public string ErrorDescription { get; set; }
    }
}
