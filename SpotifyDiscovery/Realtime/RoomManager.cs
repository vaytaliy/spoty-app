using Microsoft.AspNetCore.SignalR;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Realtime
{
    public class RoomManager : IRoomManager
    {
        private readonly IHubContext<SharedPlayerHub> _sharedPlayerHubContext;
        public RoomManager(IHubContext<SharedPlayerHub> sharedPlayerHubContext)
        {
            _sharedPlayerHubContext = sharedPlayerHubContext;
        }

        public Task AddToRoomAsHostAsync(string roomId, string connectionId)
        {
            return _sharedPlayerHubContext.Groups.AddToGroupAsync(connectionId, $"{roomId}#host");
        }

        public Task AddToRoomAsListenerAsync(string roomId, string connectionId)
        {
            return _sharedPlayerHubContext.Groups.AddToGroupAsync(connectionId, $"{roomId}#listeners");
        }

        public Task RemoveFromRoomAsync(string roomId, string connectionId)
        {
            throw new NotImplementedException();
        }

        public Task SendChatMessageToCurrentRoomAsync(string sender, string message)
        {
            throw new NotImplementedException();
        }

        public Task NotifyThisClientOnSuccessHosting(string connectionId)
        {
            return _sharedPlayerHubContext.Clients
                        .Client(connectionId)
                        .SendAsync("room-hosting-success");
        }

        public Task NotifyThisClientOnSuccessRoomJoining(string connectionId)
        {
            return _sharedPlayerHubContext.Clients.Client(connectionId).SendAsync("room-listener-success");
        }

        public Task NotifyOthersAboutConnectingAsListener(string spotifyIdOfThisListener, string newConnectedClient, string roomId)
        {
            var notificationsTasks = new Task[] {
                _sharedPlayerHubContext.Clients
                    .Group($"{roomId}#host")
                    .SendAsync("listener-connected", spotifyIdOfThisListener, newConnectedClient),

                _sharedPlayerHubContext.Clients
                    .Group($"{roomId}#listeners")
                    .SendAsync("listener-connected", spotifyIdOfThisListener, newConnectedClient)
                };
            return Task.WhenAll(notificationsTasks);
        }

        public Task SendSpotifyIdsOfThisRoom(string connectionId, IEnumerable<string> uniqueSpotifyProfileIds)
        {
            return _sharedPlayerHubContext.Clients.Client(connectionId).SendAsync("room-clients-info", uniqueSpotifyProfileIds);
        }

        public Task SendInformationAboutProfilesOfRoomToConnection(string connectionId, IEnumerable<string> uniqueSpotifyProfileIds)
        {
            return _sharedPlayerHubContext.Clients.Client(connectionId).SendAsync("room-clients-info", uniqueSpotifyProfileIds);
        }
    }
}
