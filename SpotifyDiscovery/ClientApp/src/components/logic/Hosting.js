import WebPlayerStateManager from "./WebPlayerStateManager";
import AuthLogic from "./Auth";

const Hosting = {

    connection: null,
    connectedUsersInformation: new Map(),
    roomId: null,
    deviceId: null,
    uiControls: null,
    profileId: "",
    accessToken: "",

    listen: function () {
        this.connection.on("ReceiveMessage", (message) => {
            console.log("le message received " + message);
        });

        this.connection.on("disconnect", (disconnectedConnId) => {
            console.log("somebody disconnected, " + disconnectedConnId);
        });

        this.connection.on("auth-error", (errorObj) => {
            console.log("login error");
        });

        this.connection.on("error", (message) => {
            console.log("error message", message)
        });

        this.connection.on("room-hosting-success", async () => {
            this.connectedUsersInformation = new Map();
            this.uiControls.initIsHost(true);
            console.log("successfuly hosting room");
            await this.connection.invoke("GetActiveSongInRoom", this.roomId, this.accessToken); //for testing
        });

        this.connection.on("success-settings-change", (change) => {
            console.log(change);
        });

        this.connection.on("error-settings-change", (error) => {
            console.log(error);
        });

        this.connection.on("room-host-option-changes", (change) => {
            if (change && change.description === "new_password") {
                console.log("must prompt new password to reconnect");
                this.uiControls.handleRoomLoginRequired(true);
            }
            console.log(change);
        });

        this.connection.on("room-listener-success", async () => {
            this.connectedUsersInformation = new Map(); //need to clear it after changing rooms for example
            console.log("success, listening to the room now");
            await this.connection.invoke("GetActiveSongInRoom", this.roomId, this.accessToken);
            this.uiControls.initIsHost(false)
        });

        this.connection.on("init-song-receive", async (songId) => {
            console.log("this is the current song", songId)
            console.log(this.deviceId)
            await WebPlayerStateManager.playSongById(this.accessToken, songId, this.deviceId)
        });

        this.connection.on("song-update", async (newSongId, hostConnectionId) => {
            console.log("playing new song", newSongId)
            //if (this.connection.connectionId !== hostConnectionId) {
            console.log("you are listening to new song!", newSongId)
            await WebPlayerStateManager.playSongById(window.localStorage.getItem("access_token"), newSongId, this.deviceId);
            //await this.spotiPlayer.resume();
            //}
        });

        this.connection.on("host-room-settings-information", (roomSettings) => {
            this.uiControls.handleOnloadRoomHostOptions(roomSettings);
        });

        this.connection.on("listener-disconnected", (spotifyId) => {
            this.connectedUsersInformation.delete(spotifyId);
        });

        this.connection.on("chat-message", (message, spotifyIdSender) => {
            console.log("some message received!");
            this.uiControls.handleReceiveChatMessage(message, spotifyIdSender);
        })

        this.connection.on("room-clients-info", async (connectedProfileIds) => {

            if (connectedProfileIds) {

                let tasks = []
                for (let profileId of connectedProfileIds) {
                    tasks.push(AuthLogic.getAccountInformation(window.localStorage.getItem("access_token"), profileId))
                }
                let responses = await Promise.all(tasks)

                for (let i = 0; i < responses.length; i++) {
                    this.connectedUsersInformation.set(responses[i].id, responses[i])
                }

                let jsonsTasks = []
                for (let i = 0; i < responses.length; i++) {
                    jsonsTasks.push(responses[i].json());
                }

                let parsedProfileJsons = await Promise.all(jsonsTasks)

                for (let i = 0; i < parsedProfileJsons.length; i++) {
                    this.connectedUsersInformation.set(parsedProfileJsons[i].id, parsedProfileJsons[i])
                }
                //this.connectedUsersInformation.set(profile.id, profile)
                console.log("connected people:", this.connectedUsersInformation);
                this.uiControls.handleSetConnectedPeople(this.connectedUsersInformation);

            }
        })

        this.connection.on("listener-connected", (profile) => {
            this.connectedUsersInformation.set(profile.id, profile);
            this.uiControls.handleSetConnectedPeople(this.connectedUsersInformation);
        });

        this.connection.on("host-disconnected", () => {
            console.log("the host has just quit");
        });

        this.connection.onclose(this.start);
    },

    updateState: async function (songId) {

        console.log("sending new song");
        await this.connection.invoke("UpdateSong", songId, window.localStorage.getItem('access_token'), this.roomId);
    },

    sendMessageInRoom: async function (messageData) {
        await this.connection.invoke("SendMessageInRoom", messageData, window.localStorage.getItem('access_token'), this.roomId);
    },

    associateConnectionToProfile: async function () {
        await this.connection.invoke("ConnectAccount", this.profileId);
    },

    changeRoomProperties: async function (requestedChanges) {
        await this.connection.invoke("ChangeRoomProperties", this.profileId, requestedChanges);
    },

    connectToRoom: async function (roomId, uiControls, roomPassword = "") {
        this.uiControls = uiControls;
        this.roomId = roomId;
        this.accessToken = this.uiControls.credentials.accessToken;
        this.roomPassword = roomPassword;

        console.log("requested conn")

        const userData = await AuthLogic.requestAccountId(this.accessToken)
        //await this.start()
        if (userData.id) {
            this.profileId = userData.id
            this.associateConnectionToProfile()
        } else {
            await this.uiControls.runRefreshAuthorization()
        }

        // const devicesRaw = await fetch(`https://api.spotify.com/v1/me/player/devices`, {
        //     method: 'GET',
        //     headers: {
        //         'Content-Type': 'application/json',
        //         'Authorization': `Bearer ${this.accessToken}`
        //     }
        // });

        // const devicesParsed = await devicesRaw.json(devicesRaw);

        // for (const device of devicesParsed) {
        //     if (device.is_active) {
        //         this.deviceId = device.id
        //     }
        // }

        let joinHostResultRaw = await fetch(`room_api/${this.roomId}?connId=${this.connection.connectionId}&authToken=${this.accessToken}&password=${this.roomPassword}`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${this.accessToken}`
            }
        });

        const joinHostResult = await joinHostResultRaw.json();

        if (joinHostResult && joinHostResult.error) {
            if (joinHostResult.error === "password_error") {
                console.log("password required")
                this.uiControls.handleRoomLoginRequired(true);
            }
            this.uiControls.handleRoomError(joinHostResult);
        }
    },

    start: async function () {
        try {
            await this.connection.start();
            console.log(" Connected to shared player hub");

        } catch (err) {
            console.log(err);
            setTimeout(this.start, 5000);
        }
    },

    initSignalR: async function (accessToken) {
        this.connection = new window.signalR.HubConnectionBuilder()
            .withUrl("/playerHub")    //access token will be verified on every http request so it may expire
            .configureLogging(window.signalR.LogLevel.Information)
            .build();
        await this.start(); //TODO fix this: have to await this to work fine


        this.listen();
    }
}

export default Hosting;