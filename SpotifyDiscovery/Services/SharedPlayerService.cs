using Microsoft.AspNetCore.SignalR;
using SpotifyDiscovery.Data;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Hubs;
using SpotifyDiscovery.Models;
using SpotifyDiscovery.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Services
{
    public enum RoomJoinStatuses
    {
        ROOM_NOT_EXISTS,
        SUCCESS,
        NOT_IN_FRIENDLIST,
        INCORRECT_PASSWORD
    }

    public class SharedPlayerService
    {
        private readonly IInMemoryDb _inMemoryDb;
        private readonly IRoomManager _roomManager;
        private readonly ISpotiRepository _spotiRepository;
        public SharedPlayerService(IInMemoryDb inMemoryDb, IRoomManager roomManager, ISpotiRepository spotiRepository)
        {
            _inMemoryDb = inMemoryDb;
            _roomManager = roomManager;
            _spotiRepository = spotiRepository;
        }

        public async Task TryHosting(string roomId, string connId, ProfileReadDto spotifyProfile)
        {

            if (!_inMemoryDb.RoomIsHosted(roomId))
            {
                await _inMemoryDb.MakeThisRoomHosted(roomId, spotifyProfile);
            }
            await Task.WhenAll(new Task[]
            {
                _inMemoryDb.RegisterNewConnection(connId, roomId, spotifyProfile.SpotifyId),
                _roomManager.AddToRoomAsHostAsync(roomId, connId),
                _roomManager.NotifyThisClientOnSuccessHosting(connId)
            });

            var roomInformation = await _spotiRepository.GetRoomInformation(roomId);

            if (roomInformation == null)
            {
                await _spotiRepository.CreateRoomForAccount(spotifyProfile.SpotifyId);
                roomInformation = await _spotiRepository.GetRoomInformation(roomId);
            }

            await _spotiRepository.ToggleRoomActive(roomInformation.OwnerId, true);
            var uniqueSpotifyProfileIds = await _inMemoryDb.GetAllUniqueSpotifyProfilesOfRoom(roomId);

            await Task.WhenAll(new Task[]
            {
                _roomManager.SendInformationAboutProfilesOfRoomToConnection(connId, uniqueSpotifyProfileIds),
                _roomManager.SendInformationAboutRoomToHost(connId, roomInformation)
            });

        }

        public async Task<RoomJoinStatuses> TryJoinRoom(string roomId, string connId, ProfileReadDto spotifyProfile, string password)
        {
            //if (!_inMemoryDb.RoomIsHosted(roomId))
            //{
            //    return RoomJoinStatuses.ROOM_NOT_EXISTS;
            //}

            var roomInfoTask = _spotiRepository.GetRoomInformation(roomId);
            var ownerAccountInfoTask = _spotiRepository.FindAccountBySpotifyIdAsync(roomId);

            await Task.WhenAll(new Task[] { roomInfoTask, ownerAccountInfoTask });

            if (roomInfoTask.Result.IsFriendsOnly == true)
            {
                if (IsInFriendlistOfRoomOwner())
                {
                    await ConnectUserToRequestedRoom(connId, roomId, spotifyProfile.SpotifyId);
                    return RoomJoinStatuses.SUCCESS;
                }
                else
                {
                    return RoomJoinStatuses.NOT_IN_FRIENDLIST;
                }
            }
            else if (roomInfoTask.Result.Password != null)
            {
                if (roomInfoTask.Result.Password == "")
                {
                    await ConnectUserToRequestedRoom(connId, roomId, spotifyProfile.SpotifyId);
                    return RoomJoinStatuses.SUCCESS;
                }
                else
                {
                    var personalRequirement = await GetRoomSettingsRequirements(roomId, spotifyProfile.SpotifyId);

                    if (personalRequirement.PasswordRequired == false)
                    {
                        await ConnectUserToRequestedRoom(connId, roomId, spotifyProfile.SpotifyId);
                        return RoomJoinStatuses.SUCCESS;
                    }   
                    
                    if (personalRequirement.PasswordRequired == true && roomInfoTask.Result.Password == password)
                    {
                        await _spotiRepository.MakeUserAuthorizedToRoom(roomId, spotifyProfile.SpotifyId);
                        await ConnectUserToRequestedRoom(connId, roomId, spotifyProfile.SpotifyId);
                        return RoomJoinStatuses.SUCCESS;
                    }

                    return RoomJoinStatuses.INCORRECT_PASSWORD;
                }
            }

            return RoomJoinStatuses.ROOM_NOT_EXISTS;

            bool IsInFriendlistOfRoomOwner()
            {
                if (ownerAccountInfoTask.Result.Friends.Contains(spotifyProfile.SpotifyId))
                {
                    return true;
                }

                return false;
            }

            async Task<RoomJoinStatuses> ConnectUserToRequestedRoom(string connId, string targetRoomId, string spotifyIdOfConnection)
            {
                await Task.WhenAll(new Task[]
                {
                    _inMemoryDb.RegisterNewConnection(connId, targetRoomId, spotifyIdOfConnection),
                    _roomManager.AddToRoomAsListenerAsync(targetRoomId, connId),
                    _roomManager.NotifyOthersAboutConnectingAsListener(spotifyIdOfConnection, connId, targetRoomId),
                    _roomManager.NotifyThisClientOnSuccessRoomJoining(connId)
                });

                var uniqueSpotifyProfileIds = await _inMemoryDb.GetAllUniqueSpotifyProfilesOfRoom(targetRoomId);
                await _roomManager.SendInformationAboutProfilesOfRoomToConnection(connId, uniqueSpotifyProfileIds);

                return RoomJoinStatuses.SUCCESS;
            }
        }

        public async Task<ReadRoomSettingsDto> GetRoomSettingsRequirements(string roomId, string spotifyId)
        {
            var roomInfo = await _spotiRepository.GetRoomInformation(roomId);
            var roomSettingsResponse = new ReadRoomSettingsDto();

            if (roomInfo == null)
            {
                return null;
            }

            if (roomInfo.Password != null)
            {
                if (roomInfo.Password == "")
                {
                    roomSettingsResponse.PasswordRequired = false;
                }
                else
                {
                    if (roomInfo.AuthenticatedUsers.Contains(spotifyId))
                    {
                        roomSettingsResponse.PasswordRequired = false;
                    }
                    else
                    {
                        roomSettingsResponse.PasswordRequired = true;
                    }
                }
            }
            else
            {
                roomSettingsResponse.PasswordRequired = false;
            }

            return roomSettingsResponse;

        }

        public async Task<List<Room>> GetActiveRooms(int searchStart, int searchSize)
        {
            return await _spotiRepository.GetActiveRooms(searchStart, searchSize);
        }
    }
}
