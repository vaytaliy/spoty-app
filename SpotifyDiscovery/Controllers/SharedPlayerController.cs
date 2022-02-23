using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpotifyDiscovery.Services;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Filters;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using SpotifyDiscovery.Models;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;

namespace SpotifyDiscovery.Controllers
{
    [ApiController]
    [Route("room_api")]
    public class SharedPlayerController : ControllerBase
    {
        private readonly ILogger<SharedPlayerController> _logger;
        private readonly SharedPlayerService _sharedPlayerService;
        private readonly IDistributedCache _cache;
        public SharedPlayerController(
                ILogger<SharedPlayerController> logger,
                SharedPlayerService sharedPlayerService,
                IDistributedCache cache
                )
        {
            _cache = cache;
            _logger = logger;
            _sharedPlayerService = sharedPlayerService;
        }


        [HttpGet("{roomId}")]
        [SpotifyAuthFilter]
        public async Task<IActionResult> GetPeopleConnectedToRoom(string roomId, [FromQuery] string connId, [FromQuery] string password)
        {

            var spotifyProfile = (ProfileReadDto)HttpContext.Items["User"];

            if (roomId == spotifyProfile.SpotifyId) //for host
            {
                await _sharedPlayerService.TryHosting(roomId, connId, spotifyProfile);
                return Ok();
            }

            if (roomId != spotifyProfile.SpotifyId) // for connected person
            {
                var result = await _sharedPlayerService.TryJoinRoom(roomId, connId, spotifyProfile, password);

                if (result == RoomJoinStatuses.SUCCESS)
                {
                    return Ok(new { result = "success" });
                }
                else if (result == RoomJoinStatuses.NOT_IN_FRIENDLIST)
                {
                    return Ok(new { error = "friendlist_error", description = "given user is not in friendlist" });
                }
                else if (result == RoomJoinStatuses.INCORRECT_PASSWORD)
                {
                    return Ok(new { error = "password_error", description = "incorrect password provided" });
                }

                return NotFound(new { error = "room_not_found", description = "room you're trying to connect to doesn't exist" });
            }

            return BadRequest(new { error = "unexpected_error", description = "An unexpected request error occured" });
        }

        [HttpGet("{roomId}/settings")]
        [SpotifyAuthFilter]
        public async Task<IActionResult> GetRoomSettings(string roomId)
        {
            var spotifyProfile = (ProfileReadDto)HttpContext.Items["User"];
            var requirementsResult = await _sharedPlayerService.GetRoomSettingsRequirements(roomId, spotifyProfile.SpotifyId);

            if (requirementsResult != null)
            {
                return Ok(requirementsResult);
            }

            return Ok(new {error= "room_not_found", description = "room you're trying to connect to doesn't exist" });
        }

        [HttpGet("active_rooms")]
        public async Task<IActionResult> GetActiveRooms([FromQuery] string page, CancellationToken cancellationToken)
        {

            var searchSize = 10;
            bool parseSuccess = int.TryParse(page, out int requestedPage);
            var searchStart = searchSize * requestedPage;

            if (!parseSuccess || requestedPage <= 0)
            {
                searchStart = 0;
            }

            if (searchStart > 0) searchStart -= searchSize;

            var cachedRoomsSerialized = await _cache.GetAsync($"active_pages{searchStart}", cancellationToken);
            
            if (cachedRoomsSerialized != null)
            {
                var cachedRooms = JsonSerializer.Deserialize<List<RoomGetInfoDto>>(cachedRoomsSerialized);
                return Ok(cachedRooms);
            }
            
            var activeRooms = await _sharedPlayerService.GetActiveRooms(searchStart, searchSize, cancellationToken);
            var activeRoomReadDto = activeRooms
                .Where(room =>
                room.IsActive == true &&
                room.IsPrivate == true
                )
                .Select(room => new RoomGetInfoDto
                {
                    OwnerId = room.OwnerId,
                    ActiveSong = room.ActiveSong,
                    IsPasswordRequired = room.Password != null && room.Password != "",
                    IsPrivate = room.IsPrivate
                })
                .ToList();

            if (activeRoomReadDto != null && activeRoomReadDto.Count > 0)
            {
                
                await _cache.SetAsync($"active_pages{searchStart}", JsonSerializer.SerializeToUtf8Bytes(activeRoomReadDto), 
                    new DistributedCacheEntryOptions() {AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20) }, cancellationToken);
                return Ok(activeRoomReadDto);
            }

            return Ok(new { error= "no_rooms_found", description= "couldn't find any rooms at this page"});
        }
    }
}
