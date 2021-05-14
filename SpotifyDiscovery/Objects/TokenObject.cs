using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Objects
{
    public class TokenObject
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
        [JsonPropertyName("scope")]
        public string Scope { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; } //int represents seconds until expiration
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }
}
