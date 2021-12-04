namespace SpotifyDiscovery.Models
{
    public interface ISpotifyDiscoveryDatabaseSettings
    {
        string AccountCollectionName { get; set; }
        string DatabaseName { get; set; }
        string PlayedMusicCollectionName { get; set; }
        string RoomCollectionName { get; set; }
        string ConnectionString { get; set; }
        string InMemoryDatabaseConnection { get; set; }
    }
}