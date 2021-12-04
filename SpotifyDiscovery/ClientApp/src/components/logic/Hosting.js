import WebPlayer from "./WebPlayer";
import AppInfo from "../../constants";
import AuthLogic from "./Auth";

const Hosting = {

    connection: null,
    connectedUsersInformation: new Map(),
    roomId: null,
    uiControls: null,
    spotiPlayer: null,
    deviceId: null,

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
            console.log("error message")
        });

        this.connection.on("room-hosting-success", () => {
            this.connectedUsersInformation = new Map();
            console.log("successfuly hosting room");
        });

        this.connection.on("room-listener-success", () => {
            this.connectedUsersInformation = new Map(); //need to clear it after changing rooms for example
            console.log("success, listening to the room now");
        });

        this.connection.on("song-update", async (newSongId, hostConnectionId) => {

            if (this.connection.connectionId !== hostConnectionId) {
                console.log("you are listening to new song!", newSongId)
                await WebPlayer.playSongById(window.localStorage.getItem("access_token"), newSongId, this.deviceId);
                await this.spotiPlayer.resume(); //not awaiting
            }
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
            console.log(`${profile.id} has connected, show him somehow!`);
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

    connectToRoom: async function (roomId, accessToken, uiControls, spotiPlayer, deviceId) {
        this.uiControls = uiControls;
        this.spotiPlayer = spotiPlayer;
        this.roomId = roomId;
        this.deviceId = deviceId;
        let joinHostResultRaw = await fetch(`room_api/${roomId}?connId=${this.connection.connectionId}&authToken=${accessToken}`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${accessToken}`
            }
        });

        if (joinHostResultRaw.status != 200) {
            throw {error: "error", message: "unable to host/join room"}
        }
    },

    start: async function () {
        try {
            await this.connection.start();
            console.log(" Connected.");

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