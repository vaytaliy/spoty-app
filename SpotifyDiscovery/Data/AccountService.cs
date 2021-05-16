using SpotifyDiscovery.Dtos;
using MongoDB.Bson;
using MongoDB.Driver;
using SpotifyDiscovery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Data
{
    public class AccountService
    {
        private readonly HttpClient _client;
        private readonly Db _db;
        private readonly PlaylistService _playlistService;
        private string _accessToken = "";
        public AccountService(HttpClient client, Db db, PlaylistService playlistService)
        {
            _client = client;
            _db = db;
            _playlistService = playlistService;
        }

        public async Task<string> HandleUserAccount(string accessToken)
        {
            _accessToken = accessToken;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            using var response = await _client.GetAsync("https://api.spotify.com/v1/me");

            if (response.IsSuccessStatusCode)
            {

                var responseContent = await response.Content.ReadAsStringAsync();

                ProfileReadDto userProfile = (ProfileReadDto)JsonSerializer.Deserialize(responseContent, typeof(ProfileReadDto));
                var res = await HandleProfileCreationIfNotExists(userProfile);

                if (res.Status == "Error")
                {
                    //TODO better handling
                    return res.Payload;
                }

                return res.Payload;
            }

            return null; //Handle errors here
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
            }

            var playlistId = await _playlistService.CreatePlaylist(_accessToken, userProfile.SpotifyId);

            if (playlistId == null)
            {
                return (Status: "Error", Payload: "couldn't create playlist");
            }

            await AssociatePlaylistWithAccount(userProfile.SpotifyId, playlistId);

            return (Status: "Success", Payload: playlistId);
        }

        private async Task AssociatePlaylistWithAccount(string spotifyId, string playlistId)
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
    }
}
