using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Data
{
    public interface ISpotiRepository
    {
        //Account repository
        Task<Account> FindAccountBySpotifyIdAsync(string spotifyId); //also used to get song list
        Task ConnectPlaylistToAccountAsync(string spotifyId, string playlistId);
        Task AddAccountAsync(Account account);
        Task CreateAccount(ProfileReadDto userProfile);

        //Song tracker service

        Task<PlayedMusic> FindTrackedAccount(string accountId);
        Task AddSongToTrackedAccount(string accountId, string songId);
        Task CreateTrackedAccount(string accountId, string songId);
    }
}
