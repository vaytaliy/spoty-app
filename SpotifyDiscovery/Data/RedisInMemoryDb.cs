using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Models;
using StackExchange.Redis;

namespace SpotifyDiscovery.Data
{
    public class RedisInMemoryDb : IInMemoryDb
    {
        public readonly IDatabase redisDb;
        public RedisInMemoryDb(ISpotifyDiscoveryDatabaseSettings settings)
        {
            ConfigurationOptions options = ConfigurationOptions.Parse(settings.InMemoryDatabaseConnection);
            options.ReconnectRetryPolicy = new ExponentialRetry(10000);
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(options);
            redisDb = connection.GetDatabase();
        }

        private async Task SetKeyValueAsync<T>(string key, T value)
        {
            string serializedValue = JsonSerializer.Serialize(value);

            await redisDb.StringSetAsync(key, serializedValue); //overwrites existing value
        }

        private void DeleteRecord(string key)
        {
            redisDb.KeyDelete(key);
        }

        public async Task<IEnumerable<string>> GetAllUniqueSpotifyProfilesOfRoom(string roomId)
        {
            var roomConnectionIds = await GetConnectionsOfRoom(roomId);
            var listOfConns = roomConnectionIds.ToList();
            var clientsInfoTask = new List<Task<string>>();
            var uniqueClients = new HashSet<string>();

            if (roomConnectionIds == null)
            {
                return new List<string>();
            }

            for (int i = 0; i < listOfConns.Count; i++)
            {
                var profileResponseTask = GetValueByKeyAsync<string>(roomConnectionIds[i]); // gets profile
                clientsInfoTask.Add(profileResponseTask);
            }

            var responseProfiles = await Task.WhenAll(clientsInfoTask);


            for (int i = 0; i < responseProfiles.Length; i++)
            {
                uniqueClients.Add(responseProfiles[i]);
            }

            return uniqueClients;
        }

        public async Task<T> GetValueByKeyAsync<T>(string key)
        {
            RedisValue value = await redisDb.StringGetAsync(key);

            if (value.HasValue)
            {
                var stringifiedValue = value.ToString();
                T deserializedObject = JsonSerializer.Deserialize<T>(stringifiedValue);
                return deserializedObject;
            }

            return default;
        }

        public bool RoomIsHosted(string roomId)
        {
            return redisDb.KeyExists(roomId);
        }

        public Task MakeThisRoomHosted(string roomId, ProfileReadDto spotifyProfile)
        {
            return SetKeyValueAsync(roomId, spotifyProfile);
        }

        public Task UnbindConnectionFromRoom(string connectionId, string roomId)
        {
            return redisDb.SetRemoveAsync($"room#{roomId}", connectionId);
        }

        public Task BindConnectionToRoom(string connectionId, string roomId)
        {
            return redisDb.SetAddAsync($"room#{roomId}", connectionId);
        }

        //to unload resources only stores spotify id per connection
        //then frontend can request profile information on its end (Again to save server resources)
        public Task SetSpotifyProfileIdForConnection(string connectionId, string spotifyId)
        {
            return SetKeyValueAsync(connectionId, spotifyId);
        }

        public Task<string> GetSpotifyProfileForConnection(string connectionId)
        {
            return GetValueByKeyAsync<string>(connectionId);
        }

        public void RemoveProfileInformationForConnection(string connectionId)
        {
            DeleteRecord(connectionId);
        }

        public Task SetActiveRoomOfConnection(string connectionId, string roomId)
        {
            return SetKeyValueAsync($"connect#{connectionId}", roomId);
        }

        public Task<string> GetActiveRoomOfConnection(string connectionId)
        {
            return GetValueByKeyAsync<string>($"connect#{connectionId}");
        }

        public Task RegisterNewConnection(string connectionId, string roomId, string spotifyId)
        {
            var settingUpConnection = new Task[]
             {
                BindConnectionToRoom(connectionId, roomId),
                SetSpotifyProfileIdForConnection(connectionId, spotifyId),
                SetActiveRoomOfConnection(connectionId, roomId)
             };

            return Task.WhenAll(settingUpConnection);
        }

        public async Task<List<string>> GetConnectionsOfRoom(string roomId)
        {
            var roomConnectionIds = await redisDb.SetMembersAsync($"room#{roomId}");
            
            if (roomConnectionIds != null)
            {
                var connectionsListConverted = Array.ConvertAll(roomConnectionIds, x => (string)x).ToList();
                return connectionsListConverted;
            }
            
            return default;
        }

        public void RemoveActiveRoomOfConnection(string connectionId)
        {
            DeleteRecord($"connect#{connectionId}");
        }

        public void RemoveRoom(string roomId)
        {
            DeleteRecord($"room#{roomId}");
        }
    }
}
