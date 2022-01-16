//Handles events of a player 
//and returns needed data from such events
import {deviceId} from './../SpotiPlayer'

const WebPlayerStateManager = {

    handleStateChange: (state, storedSongId) => {

        if (!state) {
            return null;
        }

        if (state.track_window.current_track.id == storedSongId) {
            return null;
        }

        const currentSong = {
            id: state.track_window.current_track.id,
            name: state.track_window.current_track.name,
            imageURL: state.track_window.current_track.album.images[1].url, //64 by 64 image
            oldSongId: storedSongId
        };

        return currentSong;
    },

    playSongById: async (accessToken, songId, deviceId) => {
        const uri = `spotify:track:${songId}`;
        console.log("im uri", uri)
        await fetch(`https://api.spotify.com/v1/me/player/play?device_id=${deviceId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`
            },
            body: JSON.stringify({
                uris: [uri]
            })
        });
    },

    transferUserPlaybackHere: async (deviceId, accessToken, isContinuePlaying = false) => {

        const res = await fetch('https://api.spotify.com/v1/me/player', {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`
            },
            body: JSON.stringify({
                device_ids: [deviceId],
                play: isContinuePlaying
            })
        });

        if (res.status == 204) {
            return { result: "success" };
        }

        if (res.status == 403) {
            return { result: "not_premium" };
        }

        if (res.status == 404) {
            return { result: "device_not_found" };
        }

        return { result: "unexpected_response_code" };
    }
}
export default WebPlayerStateManager;