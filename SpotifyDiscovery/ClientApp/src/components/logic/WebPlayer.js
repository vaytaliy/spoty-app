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

        const actualSongId = {
            id: state.track_window.current_track.id,
            name: state.track_window.current_track.name,
            imageURL: state.track_window.current_track.album.images[1].url //64 by 64 image
        };
        
        if (actualSongId.id == storedSongId) {
            return null;
        }

        return actualSongId;
    },

    // setLatestPlayingTrack: async () => {
        
    //     const token = window.localStorage.getItem("access_token");

    //     const res = await fetch('https://api.spotify.com/v1/me/player/currently-playing?market=from_token', {
    //         method: 'GET',
    //         headers: {
    //             'Content-Type': 'application/json',
    //             'Authorization': `Bearer ${token}`,
    //         },
    //     });

    //     const parsedRes = await res.json();
    //     console.log(parsedRes)

    //     if (res.status == 200) {
            
           
    //         console.log("===")
    //         console.log(parsedRes);
            
    //         return {result: "success", payload: parsedRes};
    //     }

    //     if (res.status == 204) {
            
    //         return {result: "no_track_playing"};
    //     }        
    // }
}
export default WebPlayer;