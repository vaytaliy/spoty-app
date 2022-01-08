using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SpotifyDiscovery.Data;
using SpotifyDiscovery.Dtos;
using SpotifyDiscovery.Dtos.Hub;
using SpotifyDiscovery.Filters;
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
        private readonly ISpotiRepository _spotiRepository;

        public SharedPlayerHub(AccountService accountService,
            ILogger<SharedPlayerHub> logger, IInMemoryDb inMemoryDb
            , ISpotiRepository spotiRepository
            )
        {
            _accountService = accountService;
            _logger = logger;
            _inMemoryDb = inMemoryDb;
            _spotiRepository = spotiRepository;
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
            if (user.SpotifyId == roomNumber && newSongId != null)
            {
                await _spotiRepository.SetActiveSong(roomNumber, newSongId);
                await Clients.Group($"{roomNumber}#listeners").SendAsync("song-update", newSongId, Context.ConnectionId);

                //await Clients.Group($"{roomNumber}#host").SendAsync("song-update", newSongId, Context.ConnectionId); //FOR TESTING
            }
        }

        public async Task<FriendAddDto> ToggleFriendship(string spotifyId, string friendSpotifyId)
        {
            var friendResult = await _spotiRepository.ToggleFriendship(spotifyId, friendSpotifyId);

            return friendResult;
        }

        public async Task GetActiveSongInRoom(string roomId, string accessToken)
        {
            var user = await _accountService.GetProfileFromTokenSpotify(accessToken);

            if (user == null)
            {
                return;
            }

            var room = await _spotiRepository.GetRoomInformation(roomId);

            if (room != null)
            {
                await Clients.Group(user.SpotifyId).SendAsync("init-song-receive", room.ActiveSong);
            }
        }

        private void InvokeForSpotifyIdsConnections(IEnumerable<string> spotifyIds, string methodName, object content)
        {
            var notifyTask = new List<Task>();

            foreach (var spotifyId in spotifyIds)
            {
                notifyTask.Add(Clients.Group(spotifyId).SendAsync(methodName, content));
            }

            Task.WaitAll(notifyTask.ToArray());
        }

        private async Task FlushForeignConnectionsFromRoom(string roomId, object changeMessage, IEnumerable<string> specificProfileIdsToRemove = null)
        {
            var roomConnections = await _inMemoryDb.GetConnectionsOfRoom(roomId);
            var connectionProfilesMap = new List<KeyValuePair<string, Task<string>>>();
            var connectionProfilesMapResult = new List<KeyValuePair<string, string>>();
            var connsToRemove = new List<string>();
            var profilesToRemove = new HashSet<string>();

            foreach (var connection in roomConnections)
            {
                connectionProfilesMap.Add(new KeyValuePair<string, Task<string>>(connection, _inMemoryDb.GetSpotifyProfileForConnection(connection)));
            }

            var tasks = connectionProfilesMap.Select(async connectionMap =>
            {
                var connResult = await connectionMap.Value;
                connectionProfilesMapResult.Add(new KeyValuePair<string, string>(connectionMap.Key, connResult));
            });

            await Task.WhenAll(tasks);

            foreach (var connProfilePair in connectionProfilesMapResult)
            {
                if (specificProfileIdsToRemove == null)
                {
                    if (connProfilePair.Value != roomId)
                    {
                        profilesToRemove.Add(connProfilePair.Value);
                        connsToRemove.Add(connProfilePair.Key);
                    }
                }
                else
                {
                    if (specificProfileIdsToRemove.Contains(connProfilePair.Value))
                    {
                        profilesToRemove.Add(connProfilePair.Value);
                        connsToRemove.Add(connProfilePair.Key);
                    }
                }
            }

            var removeConnTask = new List<Task>();

            foreach (var conn in connsToRemove)
            {
                removeConnTask.Add(_inMemoryDb.UnbindConnectionFromRoom(conn, roomId));
            }

            InvokeForSpotifyIdsConnections(profilesToRemove, "room-host-option-changes", changeMessage);
            Task.WaitAll(removeConnTask.ToArray());
        }

        //private async Task<PrivateToggleResultDto> ChangeRoomPrivacyAccess(string spotifyId, bool setIsPrivateRoom)
        //{
        //    var response = await _spotiRepository.ToggleRoomPrivate(spotifyId, setIsPrivateRoom);
        //    return response;
        //}

        public async Task ChangeRoomProperties(string spotifyId, ChangeRoomPropertiesReadDto requestedChange)
        {
            RoomChangeStatusResultDto roomSettingsChangeResult = new RoomChangeStatusResultDto();
            if (requestedChange.ChangeType == ChangeRoomPropertiesReadDto.PasswordChange && requestedChange.SetPassword != null)
            {
                var passwordChangePayload = await _spotiRepository.SetNewRoomPassword(spotifyId, requestedChange.SetPassword);

                roomSettingsChangeResult.ChangeType = RoomChangeStatusResultDto.PasswordChange;
                roomSettingsChangeResult.ChangePayload = passwordChangePayload;

                if (passwordChangePayload.Result == SetRoomPasswordResultDto.SetResultSuccess)
                {
                    await FlushForeignConnectionsFromRoom(spotifyId,
                        new
                        {
                            description = "new_password",
                            detailedDescription = "host requires new password"
                        });
                    await Clients.Group(spotifyId).SendAsync("success-settings-change", roomSettingsChangeResult);
                }
                else
                {
                    await Clients.Group(spotifyId).SendAsync("error-settings-change", roomSettingsChangeResult);
                }


            }
            else if (requestedChange.ChangeType == ChangeRoomPropertiesReadDto.RoomAccessChange)
            {
                var roomAccessChangePayload = await _spotiRepository.ToggleRoomPrivate(spotifyId, requestedChange.SetIsPrivateRoom);

                roomSettingsChangeResult.ChangeType = RoomChangeStatusResultDto.RoomPrivacy;
                roomSettingsChangeResult.ChangePayload = roomAccessChangePayload;

                if (roomAccessChangePayload.Result == PrivateToggleResultDto.SetResultSuccess)
                {
                    await Clients.Group(spotifyId).SendAsync("success-settings-change", roomSettingsChangeResult);
                }
                else
                {
                    await Clients.Group(spotifyId).SendAsync("error-settings-change", roomAccessChangePayload);
                }
            }
            else if (requestedChange.ChangeType == ChangeRoomPropertiesReadDto.RoomFriendsOnly)
            {
                var roomIsFriendsAccessPayload = await _spotiRepository.ToggleRoomFriendsOnly(spotifyId, requestedChange.SetIsFriendsOnly);

                roomSettingsChangeResult.ChangeType = RoomChangeStatusResultDto.RoomIsFriendsOnly;
                roomSettingsChangeResult.ChangePayload = roomIsFriendsAccessPayload;

                if (roomIsFriendsAccessPayload.Result == ToggleFriendsOnlyResultDto.SetResultSuccess)
                {
                    if (roomIsFriendsAccessPayload.FriendsOnlyAccess == true)
                    {
                        await FlushForeignConnectionsFromRoom(spotifyId,
                            new
                            {
                                description = "friends_only",
                                detailedDescription = "host changed settings to friends only, you are not in their friend list"
                            }
                            , roomIsFriendsAccessPayload.FlushProfiles);
                        await Clients.Group(spotifyId).SendAsync("success-settings-change", roomSettingsChangeResult);
                    }
                    else
                    {
                        await Clients.Group(spotifyId).SendAsync("success-settings-change", roomSettingsChangeResult);
                    }
                }
                else if (roomIsFriendsAccessPayload.Result == ToggleFriendsOnlyResultDto.ErrorResult)
                {
                    await Clients.Group(spotifyId).SendAsync("error-settings-change", roomIsFriendsAccessPayload);
                }
            }
        }


        //private async Task HandlePasswordChangeResult(RoomChangeStatusResultDto roomSettingsChangeResult, SetRoomPasswordResultDto passwordChangePayload, string spotifyId) 
        //{
        //    roomSettingsChangeResult.ChangeType = RoomChangeStatusResultDto.PasswordChange;
        //    roomSettingsChangeResult.ChangePayload = passwordChangePayload;

        //    if (passwordChangePayload.Result == SetRoomPasswordResultDto.SetResultSuccess)
        //    {
        //        await Clients.Group(spotifyId).SendAsync("success-settings-change", roomSettingsChangeResult);
        //        return;
        //    }

        //    await Clients.Group(spotifyId).SendAsync("error-settings-change", roomSettingsChangeResult);
        //}

        public async Task ConnectAccount(string spotifyId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, spotifyId);
        }

        public async Task ForceDisconnectAccount(string spotifyId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, spotifyId);
        }

        public void ForceDisconnectFromTheRoom(string roomOfConnection)
        {
            Task.WaitAll(new Task[]
            {
                _inMemoryDb.UnbindConnectionFromRoom(Context.ConnectionId, roomOfConnection),
                Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{roomOfConnection}#listeners")
            });
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
                await _spotiRepository.ToggleRoomActive(connectionSpotifyId, false);
                await Clients.Group($"{roomOfConnection}#all").SendAsync("host-disconnected"); //if this is a host
                _inMemoryDb.RemoveActiveRoomOfConnection(Context.ConnectionId);
                _inMemoryDb.RemoveRoom(Context.ConnectionId); // if hosts one
                
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
