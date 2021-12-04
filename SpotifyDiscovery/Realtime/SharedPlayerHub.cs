using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SpotifyDiscovery.Data;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Realtime;
using SpotifyDiscovery.Realtime.RealtimeUtils;
using SpotifyDiscovery.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyDiscovery.Hubs
{
    public class SharedPlayerHub : Hub
    {
        public ProfileReadDto ConnectionUserProfile { get; set; }

        private readonly AccountService _accountService;
        private readonly ILogger<SharedPlayerHub> _logger;
        private readonly IInMemoryDb _inMemoryDb;

        public SharedPlayerHub(AccountService accountService,
            ILogger<SharedPlayerHub> logger, IInMemoryDb inMemoryDb)
        {
            _accountService = accountService;
            _logger = logger;
            _inMemoryDb = inMemoryDb;
        }

        private static bool ClientBelongsToThisRoom(List<string> roomConnections, string connectionId)
        {
            if (roomConnections != null && roomConnections.Contains(connectionId))
            {
                return true;
            }
            return false;
        }

        public async Task SendMessageInRoom(ChatMessageReadDto message, string accessToken, string roomId)
        {
            var roomConnections = await _inMemoryDb.GetConnectionsOfRoom(roomId);

            // means we cant send message to room where a user doesn't belong
            if (!ClientBelongsToThisRoom(roomConnections, Context.ConnectionId))
            {
                return;
            }
            var isValidMessage = MessageValidator.ValidateMessage(message.Text);

            if (!isValidMessage)
            {
                await Clients.Caller.SendAsync("error", "message didn't pass validation");
                return;
            }

            var user = await _accountService.GetProfileFromTokenSpotify(accessToken);

            if (user != null)
            {
                message.Sender = user.SpotifyId;
                var connectionsInRoom = await _inMemoryDb.GetConnectionsOfRoom(roomId);

                if (connectionsInRoom != null)
                {
                    Task.WaitAll(new Task[]  {
                        Clients.Group($"{roomId}#host").SendAsync("chat-message", message, user.SpotifyId),
                        Clients.Group($"{roomId}#listeners").SendAsync("chat-message", message, user.SpotifyId)
                    });
                }
            }
        }

        public async Task UpdateSong(string newSongId, string accessToken, string roomNumber)
        {
            var user = await _accountService.GetProfileFromTokenSpotify(accessToken);

            if (user == null)
            {
                return;
            }

            //id equals room number - meaning thats a host
            if (user.SpotifyId == roomNumber)
            {
                await Clients.Group($"{roomNumber}#listeners").SendAsync("song-update", newSongId, Context.ConnectionId);
            }
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("client has joined");
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //cleanup in redis, from rooms user is removed automatically, notify others on disconnect!

            var roomOfConnection = await _inMemoryDb.GetActiveRoomOfConnection(Context.ConnectionId);
            var connectionSpotifyId = await _inMemoryDb.GetSpotifyProfileForConnection(Context.ConnectionId);

            if (roomOfConnection != null && roomOfConnection != connectionSpotifyId)
            {
                //specific handling for listener disconnect
                await _inMemoryDb.UnbindConnectionFromRoom(Context.ConnectionId, roomOfConnection);
                await Clients.Groups($"{roomOfConnection}#all").SendAsync("disconnect");
            }

            if (roomOfConnection != null && roomOfConnection == connectionSpotifyId)
            {
                //specific handling for host disconnect
                await Clients.Group($"{Context.ConnectionId}#all").SendAsync("host-disconnected"); //if this is a host
                _inMemoryDb.RemoveActiveRoomOfConnection(Context.ConnectionId);
                _inMemoryDb.RemoveRoom(Context.ConnectionId); // if hosts one
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
