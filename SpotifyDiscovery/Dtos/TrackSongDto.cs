using SpotifyDiscovery.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SpotifyDiscovery.Dtos
{
    public class TrackSongDto
    {
        //[Required]
        //[MaxLength(50)]
        //[JsonPropertyName(PlayedMusic.AccountIdFromJson)]
        //public string AccountId { get; set; }
        [Required]
        [MaxLength(50)]
        [JsonPropertyName("songId")]
        public string SongId { get; set; }
    }
}
