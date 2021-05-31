//Handles events of a player 
//and returns needed data from such events

const WebPlayer = {

    handleAuthError: (player, state) => {
        console.log(state); //this works ok
        if (player.initialized) {
            console.log("initialized");
        } else {
            console.log("uninitialized"); // wtf change this
        }
    },

    handleStateChange: (state, storedSongId) => {
        //actual song id -> is what in the state now
        //stored song id -> is what stored in the memory
        if (!state){
            return null;
        }

        const currentSong = {
            id: state.track_window.current_track.id,
            name: state.track_window.current_track.name,
            imageURL: state.track_window.current_track.album.images[1].url //64 by 64 image
        };
        
        if (currentSong.id == storedSongId) {
            return null;
        }

        return currentSong;
    },

    transferUserPlaybackHere: async (deviceId, accessToken, isContinuePlaying = false) => {
        //PUT https://api.spotify.com/v1/me/player/play
        console.log('token', accessToken);
        
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
            return {result: "not_premium"};
        }

        if (res.status == 404) {
            return {result: "device_not_found"};
        }

        return {result: "unexpected_response_code"};
    }
}
export default WebPlayer;