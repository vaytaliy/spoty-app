using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Data
{
    public class PlaylistService
    {
        private readonly Db _db;
        private readonly HttpClient _client;
        
        public PlaylistService(Db db, HttpClient client)
        {
            _db = db;
            _client = client;
        }

        //The playlist is only created automatically
        public async Task<string> CreatePlaylist(string accessToken, string spotifyId)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var payload = new
            {
                name = "Spoty Discovery Tray",
                description = "Playlist to save your favorite fresh tracks into"
            };

            var stringPayload = JsonSerializer.Serialize(payload);
            var requestContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            
            using var response = await _client.PostAsync($"https://api.spotify.com/v1/users/{spotifyId}/playlists", requestContent);

            if (response.IsSuccessStatusCode)
            {

                var responseContent = await response.Content.ReadAsStringAsync();

                string playlistId = JObject.Parse(responseContent)["id"].ToString(); //not ideal way but for now will leave it as is
                
                return playlistId;
            }

            return null;

        }
    }
}
