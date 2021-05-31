using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Data
{
    public class AccountService
    {
        private readonly HttpClient _client;
        private readonly Db _db;
        private string _accessToken = "";
        public AccountService(HttpClient client, Db db)
        {
            _client = client;
            _db = db;
        }

        public void AppendAuthHeader(string accessToken)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        public async Task<string> FindUserGetDatabasePlaylist(string accessToken)
        {
            _accessToken = accessToken;

            var foundAccount = await GetProfileFromTokenSpotify(accessToken);

            if (foundAccount == null)
            {
                return "couldn't find account";
            }
            var res = await HandleProfileCreationIfNotExists(foundAccount);

            if (res.Status == "Error")
            {

                return res.Payload;
            }

            return res.Payload;
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

            return null; //Handle errors here
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
            var account = await _db.Account
                .FindAsync(pre => pre.SpotifyId == userProfile.SpotifyId)
                .Result.FirstOrDefaultAsync();

            if (account == null)
            {
                var isAccountCreated = await CreateAccount(userProfile);

                if (!isAccountCreated)
                {
                    return (Status: "Error", Payload: "couldn't create account"); //TODO: better error handling
                }

                var playlistId = await CreatePlaylist(_accessToken, userProfile.SpotifyId);

                if (playlistId == null)
                {
                    return (Status: "Error", Payload: "couldn't create playlist");
                }

                await AssociatePlaylistWithAccount(userProfile.SpotifyId, playlistId);
                return (Status: "Success", Payload: playlistId);
            }

            return (Status: "Success", Payload: account.FreshPlaylistId);
        }

        public async Task AssociatePlaylistWithAccount(string spotifyId, string playlistId)
        {
            var filter = new BsonDocument("spotifyId", spotifyId);
            var update = Builders<Account>.Update.Set("freshPlaylistId", playlistId);

            await _db.Account.FindOneAndUpdateAsync<Account>(filter, update);
        }

        private async Task<bool> CreateAccount(ProfileReadDto userProfile)
        {
            Account account = new Account
            {
                SpotifyId = userProfile.SpotifyId,
                Nickname = userProfile.DisplayName
            };

            foreach (var image in userProfile.Images)
            {
                account.ProfileImages.Add(image);
            }

            try
            {
                await _db.Account.InsertOneAsync(account); //TODO handle insertion error
                return true;

            }
            catch (Exception)
            {
                return false;
            }
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

        //at first it creates request to remove song in the playlist if such exists
        //and then it adds one
        public async Task<bool> SaveToPlaylist(AddSongToPlaylistDto addSongToPlaylistDto)
        {

            var uri = $"spotify:track:{addSongToPlaylistDto.SongId}";
            var accessToken = addSongToPlaylistDto.AccessToken;
            AppendAuthHeader(accessToken);

            var isPlaylistFound = await FindSpotifyPlaylist(accessToken, addSongToPlaylistDto.PlaylistId);

            if (!isPlaylistFound)
            {
                var profile = await GetProfileFromTokenSpotify(accessToken);

                if (profile == null)
                {
                    return false;
                }

                addSongToPlaylistDto.PlaylistId = await CreatePlaylist(addSongToPlaylistDto.AccessToken, profile.SpotifyId);

                if (addSongToPlaylistDto.PlaylistId == null)
                {
                    return false;
                }

                await AssociatePlaylistWithAccount(profile.SpotifyId, addSongToPlaylistDto.PlaylistId);
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
                tracks = new List<object> {
                    new
                    {
                        uri = trackUri
                    }
                }
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
