using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpotifyDiscovery.Data;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Filters;
using SpotifyDiscovery.Services;
using System;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Controllers
{
    [ApiController]
    [Route("tracker")]
    public class SongTrackerController : ControllerBase
    {
        private readonly SongTrackerService _songTrackerService;
        private readonly AccountService _accountService;
        private readonly ILogger<SongTrackerController> _logger;
        public SongTrackerController(
            SongTrackerService songTrackerService,
            ILogger<SongTrackerController> logger,
            AccountService accountService)
        {
            _songTrackerService = songTrackerService;
            _logger = logger;
            _accountService = accountService;
        }

        [HttpPost("register")]
        [SpotifyAuthFilter]
        public async Task<IActionResult> Register([FromBody] TrackSongDto trackSongDto)
        {
            var spotifyProfile = (ProfileReadDto)HttpContext.Items["User"];

            try
            {
                var trackerResult = await _songTrackerService.SaveNewSongId(
                    spotifyProfile.SpotifyId, trackSongDto.SongId, spotifyProfile);

                return Ok(new { result = trackerResult });
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        [HttpPost("add_to_playlist")]
        [SpotifyAuthFilter]
        public async Task<IActionResult> AddToPlaylist(AddSongToPlaylistDto addSongToPlaylistDto)
        {
            var spotifyProfile = (ProfileReadDto)HttpContext.Items["User"];

            var playlistId = await _accountService.FindUserGetDatabasePlaylist(spotifyProfile);
            addSongToPlaylistDto.PlaylistId = playlistId;

            var isSuccessful = await _accountService.SaveToPlaylist(addSongToPlaylistDto, spotifyProfile);

            if (isSuccessful)
            {
                return Ok();
            }
            return Forbid();
        }

        [HttpGet("get_playlist")]
        [SpotifyAuthFilter]
        public async Task<IActionResult> GetPlaylist()
        {
            var spotifyProfile = (ProfileReadDto)HttpContext.Items["User"];
            var playlistId = await _accountService.FindUserGetDatabasePlaylist(spotifyProfile);

            return Ok(new { playlistId });
        }
    }
}
