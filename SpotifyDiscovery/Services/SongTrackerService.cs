using MongoDB.Bson;
using MongoDB.Driver;
using SpotifyDiscovery.Data;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Services
{
    public class SongTrackerService
    {
        private readonly ISpotiRepository _spotiRepository;
        public SongTrackerService(ISpotiRepository spotiRepository)
        {
            _spotiRepository = spotiRepository;
        }

        public async Task<string> SaveNewSongId(string accountId, string songId, ProfileReadDto spotifyProfile)
        {
            var userAccount = await _spotiRepository.FindTrackedAccount(accountId);

            if (userAccount != null)
            {
                if (userAccount.SongIdList.Contains(songId))
                {
                    return "song_exists";
                }

                await _spotiRepository.AddSongToTrackedAccount(accountId, songId);
                return "new_song";
            }

            if (userAccount == null)
            {
                await _spotiRepository.CreateTrackedAccount(accountId, songId);
                return "new_id";
            }

            return "unexpected_error";
        }
    }
}
