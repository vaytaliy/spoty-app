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
    }
}
export default WebPlayer;