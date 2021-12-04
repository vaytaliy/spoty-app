using SpotifyDiscovery.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Data
{
    public interface IInMemoryDb
    {
        //Task AddKeyValueAsync<T>(string key, T value);
        //Task<T> GetValueByKeyAsync<T>(string key);

        //bool KeyExists(string key);

        //void DeleteRecord(string key);

        Task<IEnumerable<string>> GetAllUniqueSpotifyProfilesOfRoom(string roomId);
        bool RoomIsHosted(string roomId);
        Task MakeThisRoomHosted(string roomId, ProfileReadDto spotifyProfile);
        Task UnbindConnectionFromRoom(string connectionId, string roomId);
        Task BindConnectionToRoom(string connectionId, string roomId);
        Task SetSpotifyProfileIdForConnection(string connectionId, string spotifyId);
        Task SetActiveRoomOfConnection(string connectionId, string roomId);
        Task RegisterNewConnection(string connectionId, string roomId, string spotifyId);
        Task<List<string>> GetConnectionsOfRoom(string roomId);
        Task<string> GetActiveRoomOfConnection(string connectionId);
        Task<string> GetSpotifyProfileForConnection(string connectionId);
        void RemoveActiveRoomOfConnection(string connectionId);
        void RemoveProfileInformationForConnection(string connectionId);
        void RemoveRoom(string roomId);
    }
}
