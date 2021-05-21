using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpotifyDiscovery.Data;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Controllers
{
    [ApiController]
    [Route("tracker")]
    public class SongTrackerController : ControllerBase
    {
        private readonly SongTrackerService _songTrackerService;
        private readonly AccountService _accountService;
        private readonly PlaylistService _playlistService;
        private readonly ILogger<SongTrackerController> _logger;
        private readonly IMapper _mapper;
        public SongTrackerController(
            SongTrackerService songTrackerService,
            ILogger<SongTrackerController> logger,
            AccountService accountService,
            IMapper mapper,
            PlaylistService playlistService)
        {
            _songTrackerService = songTrackerService;
            _logger = logger;
            _mapper = mapper;
            _accountService = accountService;
            _playlistService = playlistService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> TrackSong([FromBody] TrackSongDto trackSongDto)
        {     
            var trackerResult = await _songTrackerService.SaveNewSongId(
                trackSongDto.AccountId, trackSongDto.SongId);

            return Ok(
                    new
                    {
                        result = trackerResult
                    }
                );
        }

        [HttpPost("add_to_playlist")]
        public async Task<IActionResult> AddToPlaylist(AddSongToPlaylistDto addSongToPlaylistDto)
        {
            //var AuthenticationHeaderValue.Parse("Bearer");
            var playlistId = await _accountService.FindUserGetDatabasePlaylist(addSongToPlaylistDto.AccessToken);
            addSongToPlaylistDto.PlaylistId = playlistId;

            var isSuccessful = await _playlistService.SaveToPlaylist(addSongToPlaylistDto);
            
            if (isSuccessful)
            {
                return Ok();
            }

            return Forbid();
        }

        [HttpPost("test")]
        public IActionResult Test()
        {

            return Ok(
                    new
                    {
                       data = "it works"
                    }
                );
        }
    }
}
