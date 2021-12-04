using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Models
{
    //different settings to support dev and prod
    public class SpotifyDiscoveryDatabaseSettings : ISpotifyDiscoveryDatabaseSettings
    {
        public string AccountCollectionName { get; set; }
        public string PlayedMusicCollectionName { get; set; }
        public string RoomCollectionName { get; set; }
        public string DatabaseName { get; set; }
        public string ConnectionString { get; set; }
        public string InMemoryDatabaseConnection { get; set; }
    }
}
