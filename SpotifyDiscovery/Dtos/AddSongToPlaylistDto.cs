using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Dtos
{
    public class AddSongToPlaylistDto
    {
        //string songId, string playlistId, string accessToken
        [JsonPropertyName("songId")]
        public string SongId { get; set; }
        [JsonPropertyName("playlistId")]
        public string PlaylistId { get; set; }
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }
    }
}
