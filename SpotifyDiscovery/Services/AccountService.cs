using Newtonsoft.Json.Linq;
using SpotifyDiscovery.Data;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Realtime;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Services
{
    public class AccountService
    {
        private readonly HttpClient _client;
        private readonly ISpotiRepository _spotiRepository;
        public string AccessToken { get; set; } = "";

        public AccountService(HttpClient client,ISpotiRepository spotiRepository)
        {
            _client = client;
            _spotiRepository = spotiRepository;
            
        }

        public void AppendAuthHeader(string accessToken)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        public async Task<string> FindUserGetDatabasePlaylist(ProfileReadDto spotifyProfile)
        {

            var (Status, Payload) = await HandleProfileCreationIfNotExists(spotifyProfile);
            return Payload;
        }

        public async Task<ProfileReadDto> GetProfileFromTokenSpotify(string accessToken)
        {
            AppendAuthHeader(accessToken);

            using var response = await _client.GetAsync("https://api.spotify.com/v1/me");

            if (response.IsSuccessStatusCode)
            {

                var responseContent = await response.Content.ReadAsStringAsync();
                ProfileReadDto userProfile = (ProfileReadDto)JsonSerializer.Deserialize(responseContent, typeof(ProfileReadDto));

                return userProfile;
            }

            return null;
        }

        public async Task<string> CreatePlaylist(string accessToken, string spotifyId)
        {
            AppendAuthHeader(accessToken);

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

        private async Task<(string Status, string Payload)> HandleProfileCreationIfNotExists(ProfileReadDto userProfile)
        {
            var account = await _spotiRepository.FindAccountBySpotifyIdAsync(userProfile.SpotifyId);

            if (account == null)
            {
                await _spotiRepository.CreateAccount(userProfile);

                var playlistId = await CreatePlaylist(AccessToken, userProfile.SpotifyId);

                if (playlistId == null)
                {
                    return (Status: "Error", Payload: "couldn't create playlist");
                }
                var playlistTask = _spotiRepository.ConnectPlaylistToAccountAsync(userProfile.SpotifyId, playlistId);
                Task.WaitAll(playlistTask);

                return (Status: "Success", Payload: playlistId);
            }

            return (Status: "Success", Payload: account.FreshPlaylistId);
        }


        public async Task<bool> FindSpotifyPlaylist(string accessToken, string playlistId)
        {

            var fields = "id"; //only requesting for id 

            AppendAuthHeader(accessToken);
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

        public async Task<bool> SaveToPlaylist(AddSongToPlaylistDto addSongToPlaylistDto, ProfileReadDto spotifyProfile)
        {

            var uri = $"spotify:track:{addSongToPlaylistDto.SongId}";
            var accessToken = addSongToPlaylistDto.AccessToken;
            AppendAuthHeader(accessToken);

            var isPlaylistFound = await FindSpotifyPlaylist(accessToken, addSongToPlaylistDto.PlaylistId);

            if (!isPlaylistFound)
            {

                addSongToPlaylistDto.PlaylistId = await CreatePlaylist(addSongToPlaylistDto.AccessToken, spotifyProfile.SpotifyId);

                if (addSongToPlaylistDto.PlaylistId == null)
                {
                    return false;
                }

                await _spotiRepository.ConnectPlaylistToAccountAsync(spotifyProfile.SpotifyId, addSongToPlaylistDto.PlaylistId);
            }

            RemoveTrackFromSpotifyPlaylist(addSongToPlaylistDto.PlaylistId, uri, accessToken);

            using var response = await _client.PostAsync($"https://api.spotify.com/v1/playlists/{addSongToPlaylistDto.PlaylistId}/tracks?uris={uri}", null);

            if (response.IsSuccessStatusCode)
            {
                //Track was created in the playlist
                return true;
            }
            //if status code 403 - error or playlist size is 10 000
            return false;
        }

        public void RemoveTrackFromSpotifyPlaylist(string playlistId, string trackUri, string accessToken)
        {

            AppendAuthHeader(accessToken);

            var payload = new
            {
                tracks = new List<object> { new { uri = trackUri } }
            };

            var stringPayload = JsonSerializer.Serialize(payload);
            var requestContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"https://api.spotify.com/v1/playlists/{playlistId}/tracks"),
                Content = requestContent,
            };

            _client.SendAsync(request);

        }
    }
}
