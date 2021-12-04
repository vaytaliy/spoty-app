using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpotifyDiscovery.Services;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Filters;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Controllers
{
    [ApiController]
    [Route("room_api")]
    public class SharedPlayerController : ControllerBase
    {
        private readonly ILogger<SharedPlayerController> _logger;
        private readonly SharedPlayerService _sharedPlayerService;
        public SharedPlayerController(
                ILogger<SharedPlayerController> logger,
                SharedPlayerService sharedPlayerService)
        {
            _logger = logger;
            _sharedPlayerService = sharedPlayerService;
        }


        [HttpGet("{roomId}")]
        [SpotifyAuthFilter]
        public async Task<IActionResult> GetPeopleConnectedToRoom(string roomId, [FromQuery] string connId)
        {

            var spotifyProfile = (ProfileReadDto)HttpContext.Items["User"];

            if (roomId == spotifyProfile.SpotifyId) //for host
            {
                await _sharedPlayerService.TryHosting(roomId, connId, spotifyProfile);
                //Utilities.TestConnect.ConnIdAlreadyHosting = true;
                return Ok();
            }

            if (roomId != spotifyProfile.SpotifyId) // for connected person
            {
                var result = await _sharedPlayerService.TryJoinRoom(roomId, connId, spotifyProfile);

                if (result == RoomJoinStatuses.SUCCESS)
                {
                    return Ok();
                }

                return NotFound(result);
            }

            return BadRequest("An unexpected request error occured");
        }
    }
}
