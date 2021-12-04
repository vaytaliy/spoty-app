using MongoDB.Bson;
using MongoDB.Driver;
using SpotifyDiscovery.Data;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Models;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Data
{
    public class DbSpotiRepository : ISpotiRepository
    {
        private readonly Db _db;
        public DbSpotiRepository(Db db)
        {
            _db = db;
        }

        public async Task AddAccountAsync(Account userProfile)
        {
            await _db.Account.InsertOneAsync(userProfile);
        }

        public async Task CreateAccount(ProfileReadDto userProfile)
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

            await AddAccountAsync(account);
        }

        public async Task AddSongToTrackedAccount(string accountId, string songId)
        {
            var filter = new BsonDocument(PlayedMusic.AccountIdFromJson, accountId);
            var update = Builders<PlayedMusic>.Update.Push(PlayedMusic.SongListFromJson, songId);

            await _db.PlayedMusic.UpdateOneAsync(filter, update);
        }

        public async Task ConnectPlaylistToAccountAsync(string spotifyId, string playlistId)
        {
            var filter = new BsonDocument("spotifyId", spotifyId);
            var update = Builders<Account>.Update.Set("freshPlaylistId", playlistId);

            await _db.Account.FindOneAndUpdateAsync<Account>(filter, update);
        }

        public async Task CreateTrackedAccount(string accountId, string songId = "no_song")
        {
            var newTrackedAccount = new PlayedMusic
            {
                AccountId = accountId,
                SongIdList = new BsonArray()
            };

            if (songId != "no_song")
            {
                newTrackedAccount.SongIdList.Add(songId);
            }

            await _db.PlayedMusic.InsertOneAsync(newTrackedAccount);
        }

        public async Task<Account> FindAccountBySpotifyIdAsync(string spotifyId)
        {
            var account = await _db.Account
                .FindAsync(pre => pre.SpotifyId == spotifyId)
                .Result.FirstOrDefaultAsync();

            if (account != null)
            {
                return account;
            }

            return null;
        }

        public async Task<PlayedMusic> FindTrackedAccount(string accountId)
        {
            var foundAccount = await _db.PlayedMusic
                .FindAsync(playedMusic => playedMusic.AccountId == accountId)
                .Result.FirstOrDefaultAsync();

            return foundAccount;
        }

       // public async Task<>
    }
}
