using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Dtos.Hub;
using SpotifyDiscovery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Data
{
    public enum FriendUpdate
    {
        Added = 1,
        Removed = 2,
        AccountNotFound = 3
    }

    public interface ISpotiRepository
    {
        //Account repository
        Task<Account> FindAccountBySpotifyIdAsync(string spotifyId); //also used to get song list
        Task ConnectPlaylistToAccountAsync(string spotifyId, string playlistId);
        Task AddAccountAsync(Account account);
        Task CreateAccount(ProfileReadDto userProfile);
        Task CreateRoomForAccount(string spotifyId);
        //Song tracker service

        Task<PlayedMusic> FindTrackedAccount(string accountId);
        Task AddSongToTrackedAccount(string accountId, string songId);
        Task CreateTrackedAccount(string accountId, string songId);

        //Hub service
        Task<FriendAddDto> ToggleFriendship(string spotifyId, string friendSpotifyId);
        Task<Room> GetRoomInformation(string roomId);
        Task<SetRoomPasswordResultDto> SetNewRoomPassword(string roomId, string newRoomPassword);
        Task<ToggleFriendsOnlyResultDto> ToggleRoomFriendsOnly(string roomId, bool isFriendsOnlyFlag);
        Task<PrivateToggleResultDto> ToggleRoomPrivate(string roomId, bool isPrivateRoomFlag);
        Task MakeUserAuthorizedToRoom(string roomId, string spotifyId);
        Task ToggleRoomActive(string spotifyId, bool roomIsActive);
        Task<List<Room>> GetActiveRooms(int searchStart, int searchSize);
        Task SetActiveSong(string roomId, string songId);
    }
}
