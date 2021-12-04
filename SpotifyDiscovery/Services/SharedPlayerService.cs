using Microsoft.AspNetCore.SignalR;
using SpotifyDiscovery.Data;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Hubs;
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
        SUCCESS
    }
    
    public class SharedPlayerService
    {
        private readonly IInMemoryDb _inMemoryDb;
        private readonly IRoomManager _roomManager;
        public SharedPlayerService(IInMemoryDb inMemoryDb, IRoomManager roomManager)
        {
            _inMemoryDb = inMemoryDb;
            _roomManager = roomManager;
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

            var uniqueSpotifyProfileIds = await _inMemoryDb.GetAllUniqueSpotifyProfilesOfRoom(roomId);
            await _roomManager.SendInformationAboutProfilesOfRoomToConnection(connId, uniqueSpotifyProfileIds);
        }

        public async Task<RoomJoinStatuses> TryJoinRoom(string roomId, string connId, ProfileReadDto spotifyProfile)
        {
            if (!_inMemoryDb.RoomIsHosted(roomId))
            {
                return RoomJoinStatuses.ROOM_NOT_EXISTS;
            }
            
            await Task.WhenAll(new Task[]
            {
                _inMemoryDb.RegisterNewConnection(connId, roomId, spotifyProfile.SpotifyId),
                _roomManager.AddToRoomAsListenerAsync(roomId, connId),
                _roomManager.NotifyOthersAboutConnectingAsListener(spotifyProfile.SpotifyId, connId, roomId),
                _roomManager.NotifyThisClientOnSuccessRoomJoining(connId)
            });
            
            var uniqueSpotifyProfileIds = await _inMemoryDb.GetAllUniqueSpotifyProfilesOfRoom(roomId);
            await _roomManager.SendInformationAboutProfilesOfRoomToConnection(connId, uniqueSpotifyProfileIds);

            return RoomJoinStatuses.SUCCESS;
        }
    }
}
