using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpotifyDiscovery.Data;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Controllers
{
    [ApiController]
    [Route("tracker")]
    public class SongTrackerController : ControllerBase
    {
        private readonly SongTrackerService _songTrackerService;
        private readonly ILogger<SongTrackerController> _logger;
        private readonly IMapper _mapper;
        public SongTrackerController(
            SongTrackerService songTrackerService,
            ILogger<SongTrackerController> logger,
            IMapper mapper)
        {
            _songTrackerService = songTrackerService;
            _logger = logger;
            _mapper = mapper;
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
