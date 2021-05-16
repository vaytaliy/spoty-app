using MongoDB.Bson;
using MongoDB.Driver;
using SpotifyDiscovery.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Data
{
    public class SongTrackerService
    {
        private readonly Db _db;

        public SongTrackerService(Db db)
        {
            _db = db;
        }

        /// <summary> 
        /// <para>
        /// accountId: string identifier of spotify acc number
        /// songId: id of a song that needs to be saved into the system
        /// </para>
        /// returns one of the following:
        /// 
        /// <para>"new_song" : didn't exist in db</para>
        /// <para>"song_exists" : song already exists in db</para>
        /// <para>"error": error saving </para>
        /// <para>"new_id": new user created + song added</para>
        /// </summary>
        /// 

        // mongodb c# driver reference
        // http://mongodb.github.io/mongo-csharp-driver/2.0/reference/driver/crud/writing/
        public async Task<string> SaveNewSongId(string accountId, string songId)
        {
            var foundAccountAndSong = await _db.PlayedMusic
                .FindAsync(playedMusic => playedMusic.AccountId == accountId
                && playedMusic.SongIdList
                .Contains(songId)
            ).Result.FirstOrDefaultAsync();

            if (foundAccountAndSong != null)
            {
                return "song_exists";
            }

            var foundAccount = await _db.PlayedMusic
                .FindAsync(playedMusic => playedMusic.AccountId == accountId)
                .Result.FirstOrDefaultAsync();

            if (foundAccount == null)
            {
                var bsonDoc = CreateTrackedAccount(accountId);
                bsonDoc.SongIdList.Add(songId);

                await _db.PlayedMusic.InsertOneAsync(bsonDoc);
                return "new_id";
            }

            var filter = new BsonDocument(PlayedMusic.AccountIdFromJson, accountId);
            var update = Builders<PlayedMusic>.Update.Push(PlayedMusic.SongListFromJson, songId);

            var music = await _db.PlayedMusic.FindOneAndUpdateAsync<PlayedMusic>(filter, update);
            return "new_song";
        }

        //TODO: Add ability to sync playlists
        public int SaveToPlaylist(string songId, string ownerId)
        {
            //
            return -1;
        }

        private static PlayedMusic CreateTrackedAccount(string accountId)
        {
            var doc = new PlayedMusic
            {
                AccountId = accountId,
                SongIdList = new BsonArray()
            };

            return doc;
        }
    }
}
