using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SpotifyDiscovery.Data;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Dtos.Hub;
using SpotifyDiscovery.Models;
using System.Collections.Generic;
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
                Nickname = userProfile.DisplayName,
                Friends = new BsonArray()
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
            var filter = new BsonDocument(Account.SpotifyIdBsonName, spotifyId);
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



        public async Task<FriendAddDto> ToggleFriendship(string spotifyId, string friendSpotifyId)
        {

            var foundAccount = await _db.Account
                .FindAsync(pre => pre.SpotifyId == spotifyId)
                .Result.FirstOrDefaultAsync();

            if (foundAccount == null)
            {
                return new FriendAddDto() { Result = "error", Description = "unable to find your account" };
            }

            var filter = Builders<Account>.Filter.Eq(pre => pre.SpotifyId, spotifyId);

            if (foundAccount.Friends.Contains(friendSpotifyId))
            {
                await RemoveFriend(friendSpotifyId);
                return new FriendAddDto() { Result = "removed", Description = $"removed {friendSpotifyId} from friends" };
            }
            else
            {
                await AddFriend(friendSpotifyId);
                return new FriendAddDto() { Result = "added", Description = $"added {friendSpotifyId} to friends" };
            }

            async Task RemoveFriend(string friendName)
            {
                var update = Builders<Account>
                    .Update.Pull(pre => pre.Friends, friendName);

                await _db.Account.UpdateOneAsync(filter, update);
            }

            async Task AddFriend(string friendName)
            {
                var update = Builders<Account>
                    .Update.Push(pre => pre.Friends, friendName);

                await _db.Account.UpdateOneAsync(filter, update);
            }
        }

        public async Task<Room> GetRoomInformation(string roomId)
        {
            var foundRoom = await _db.Room.Find(pre => pre.OwnerId == roomId).FirstOrDefaultAsync();
            if (foundRoom == null)
            {
                return null;
            }
            return foundRoom;
        }

        public async Task CreateRoomForAccount(string spotifyId)
        {
            await _db.Room.InsertOneAsync(new Room()
            {
                OwnerId = spotifyId,
                RoomName = spotifyId,
                IsFriendsOnly = false,
                AuthenticatedUsers = new BsonArray(),
                IsPrivate = true,
                Password = ""
            });
        }

        public async Task<SetRoomPasswordResultDto> SetNewRoomPassword(string roomId, string newRoomPassword)
        {
            var room = await GetRoomInformation(roomId);

            if (room == null)
            {
                return new SetRoomPasswordResultDto()
                {
                    Result = SetRoomPasswordResultDto.ErrorResult,
                    Error = "room",
                    ErrorDescription = $"Unable to change password. Room with id {roomId} doesn't exist"
                };
            }

            if (room.Password == newRoomPassword || newRoomPassword.Length > Room.MaxPasswordLength)
            {
                return new SetRoomPasswordResultDto()
                {
                    Result = SetRoomPasswordResultDto.ErrorResult,
                    Error = "password",
                    ErrorDescription = $"Provided same password or password is longer than allowed {Room.MaxPasswordLength} symbols length"
                };
            }
            //accepts empty password, therefore no auth is necessary
            await _db.Room.FindOneAndUpdateAsync(pre => pre.OwnerId == roomId, Builders<Room>.Update.Set(Room.PasswordBson, newRoomPassword));
            return new SetRoomPasswordResultDto()
            {
                Result = SetRoomPasswordResultDto.SetResultSuccess,
                NewPassword = newRoomPassword
            };
        }

        //if toggled true returns list of authenticated people not related to friends
        private static List<string> GetForeignAccountsInRoom(Room room, Account account)
        {
            var accountsToFlushFromRoom = new List<string>();

            foreach (var authedSpotifyId in room.AuthenticatedUsers)
            {
                if (!account.Friends.Contains(authedSpotifyId))
                {
                    accountsToFlushFromRoom.Add(authedSpotifyId.AsString);
                }
            }

            return accountsToFlushFromRoom;
        }

        public async Task<ToggleFriendsOnlyResultDto> ToggleRoomFriendsOnly(string roomId, bool isFriendsOnlyFlag)
        {
            var account = await FindAccountBySpotifyIdAsync(roomId);
            var room = await GetRoomInformation(roomId);

            if (account != null && room != null)
            {
                if (room.IsFriendsOnly == false && isFriendsOnlyFlag == true)
                {
                    var updatedRoomBson = await _db.Room.FindOneAndUpdateAsync(pre => pre.OwnerId == roomId, Builders<Room>.Update
                        .Set(Room.IsFriendsOnlyBson, true)
                        .Set(Room.IsPrivateBson, true), options: new FindOneAndUpdateOptions<Room, BsonDocument>
                        {
                            IsUpsert = true,
                            ReturnDocument = ReturnDocument.After
                        });//here is some issue
                    var updatedRoom = BsonSerializer.Deserialize<Room>(updatedRoomBson);
                    var accountsToFlushFromRoom = GetForeignAccountsInRoom(updatedRoom, account);

                    return new ToggleFriendsOnlyResultDto()
                    {
                        Result = ToggleFriendsOnlyResultDto.SetResultSuccess,
                        FriendsOnlyAccess = updatedRoom.IsFriendsOnly,
                        FlushProfiles = accountsToFlushFromRoom
                    };
                }
                else if (room.IsFriendsOnly == true && isFriendsOnlyFlag == false)
                {
                    var updatedRoomBson = await _db.Room.FindOneAndUpdateAsync(pre => pre.OwnerId == roomId, Builders<Room>.Update
                        .Set(Room.IsFriendsOnlyBson, false), options: new FindOneAndUpdateOptions<Room, BsonDocument>
                        {
                            IsUpsert = true,
                            ReturnDocument = ReturnDocument.After
                        });

                    var updatedRoom = BsonSerializer.Deserialize<Room>(updatedRoomBson);

                    return new ToggleFriendsOnlyResultDto()
                    {
                        Result = ToggleFriendsOnlyResultDto.SetResultSuccess,
                        FriendsOnlyAccess = updatedRoom.IsFriendsOnly
                    };
                }
            }

            return new ToggleFriendsOnlyResultDto()
            {
                Result = ToggleFriendsOnlyResultDto.ErrorResult
            };
        }

        public async Task<PrivateToggleResultDto> ToggleRoomPrivate(string roomId, bool isPrivateRoomFlag)
        {
            var room = GetRoomInformation(roomId);
            var account = FindAccountBySpotifyIdAsync(roomId);

            Task.WaitAll(new Task[] { room, account });

            var foundRoom = room.Result;
            var foundAccount = account.Result;

            if (foundRoom != null && foundAccount != null)
            {
                if (isPrivateRoomFlag != foundRoom.IsPrivate)
                {
                    var updatedRoomBson = await _db.Room.FindOneAndUpdateAsync(pre => pre.OwnerId == roomId, Builders<Room>.Update
                        .Set(Room.IsPrivateBson, isPrivateRoomFlag), options: new FindOneAndUpdateOptions<Room, BsonDocument>
                        {
                            IsUpsert = true,
                            ReturnDocument = ReturnDocument.After
                        });

                    var updatedRoom = BsonSerializer.Deserialize<Room>(updatedRoomBson);
                    return new PrivateToggleResultDto() { Result = PrivateToggleResultDto.SetResultSuccess, AccessChange = updatedRoom.IsPrivate };
                }

                return new PrivateToggleResultDto() { Result = PrivateToggleResultDto.SetResultNoChange };
            }

            return new PrivateToggleResultDto()
            {
                Result = PrivateToggleResultDto.SetResultNoChange,
                ErrorDescription = "no information on room or account is available"
            };
        }

        public async Task ToggleRoomActive(string spotifyId, bool roomIsActive)
        {
            await _db.Room.FindOneAndUpdateAsync(pre => pre.OwnerId == spotifyId, Builders<Room>
                .Update.Set(Room.RoomIsActive, roomIsActive));
        }

        public async Task MakeUserAuthorizedToRoom(string roomId, string spotifyId)
        {
            await _db.Room.FindOneAndUpdateAsync(pre => pre.OwnerId == roomId, Builders<Room>
                .Update.Push(Room.AuthenticatedUsersBson, spotifyId));
        }

        public async Task<List<Room>> GetActiveRooms(int searchStart, int searchSize)
        {
            var rooms = await _db.Room.Find(pre => pre.IsActive == true).Skip(searchStart).Limit(searchSize).ToListAsync();
            return rooms;
        }

        public async Task SetActiveSong(string roomId, string songId)
        {
            await _db.Room.FindOneAndUpdateAsync(pre => pre.OwnerId == roomId, Builders<Room>.Update
                        .Set(Room.ActiveSongBson, songId));
        }
    }
}
