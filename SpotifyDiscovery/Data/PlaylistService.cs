using Newtonsoft.Json.Linq;
using SpotifyDiscovery.Dtos;
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
        private readonly AccountService _accountService;
        
        public PlaylistService(Db db, HttpClient client, AccountService accountService)
        {
            _db = db;
            _client = client;
            _accountService = accountService;
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

        ///<summary>
        ///<para>Check if playlist exists</para>
        ///<para>If it returns back an ID - then it exists</para>
        ///true | false
        ///</summary>
        public async Task<bool> FindSpotifyPlaylist(string accessToken, string playlistId)
        {
            //
            var fields = "id"; //only asking for id 
            
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await _client.GetAsync($"https://api.spotify.com/v1/playlists/{playlistId}?fields={fields}");

            if (response.IsSuccessStatusCode)
            {
                // was created
                var responseContent = await response.Content.ReadAsStringAsync();

                PlaylistReadDto userProfile = (PlaylistReadDto)JsonSerializer.Deserialize(responseContent, typeof(PlaylistReadDto));
                if (userProfile.PlaylistId != null)
                {
                    return true;
                }
            }
            return false;

        }

        //TODO: Add ability to sync playlists
        public async Task<bool> SaveToPlaylist(AddSongToPlaylistDto addSongToPlaylistDto)
        {
            //TODO first find playlist - if it exists in spotify services
            // if not found - create playlist and then
            //_accountService.AssociatePlaylistWithAccount()
            //

            var uri = $"spotify:track:{addSongToPlaylistDto.SongId}";
            var accessToken = addSongToPlaylistDto.AccessToken;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var isPlaylistFound = await FindSpotifyPlaylist(accessToken, addSongToPlaylistDto.PlaylistId);

            if (!isPlaylistFound)
            {
                var profile = await _accountService.GetProfileFromTokenSpotify(accessToken);

                if (profile == null)
                {
                    return false;
                }

                addSongToPlaylistDto.PlaylistId = await CreatePlaylist(addSongToPlaylistDto.AccessToken, profile.SpotifyId);

                if (addSongToPlaylistDto.PlaylistId == null)
                {
                    return false;
                }

                await _accountService.AssociatePlaylistWithAccount(profile.SpotifyId, addSongToPlaylistDto.PlaylistId);
            }

            using var response = await _client.PostAsync($"https://api.spotify.com/v1/playlists/{addSongToPlaylistDto.PlaylistId}/tracks?uris={uri}", null);

            if (response.IsSuccessStatusCode)
            {
                // was created
                return true;
            }

            return false;
            //403 message - error or playlist size is 10 000
        }
    }
}
