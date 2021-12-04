using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Realtime
{
    public interface IRoomManager
    {
        Task AddToRoomAsHostAsync(string roomId, string connectionId);
        Task AddToRoomAsListenerAsync(string roomId, string connectionId);
        Task RemoveFromRoomAsync(string roomId, string connectionId);
        Task SendChatMessageToCurrentRoomAsync(string sender, string message);
        Task NotifyThisClientOnSuccessHosting(string connectionId);
        Task NotifyThisClientOnSuccessRoomJoining(string connectionId);
        Task NotifyOthersAboutConnectingAsListener(string spotifyIdOfThisListener, string newConnectedClient, string roomId);
        Task SendInformationAboutProfilesOfRoomToConnection(string connectionId, IEnumerable<string> uniqueSpotifyProfileIds);
    }
}
