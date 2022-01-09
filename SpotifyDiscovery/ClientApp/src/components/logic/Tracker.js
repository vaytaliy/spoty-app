import { trackedMusicThisSession } from '../storage/InMemoryStorage';

const Tracker = {

    handleSongRegistrationRequest: async (requestData, accessToken) => {

        if (trackedMusicThisSession.has(requestData.songId)) {

            return { result: "in_memory" };
        }

        if (!requestData.songId) {
            return { result: "failure" }
        }

        const res = await fetch(`tracker/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`
            },
            body: JSON.stringify({
                accountId: requestData.accountId,
                songId: requestData.songId
            })
        });

        if (res.status != 200) {
            return { result: "failure" };
        }

        trackedMusicThisSession.add(requestData.songId);
        const parsedRes = await res.json();

        return parsedRes;
    },

    saveSongToPlaylistRequest: async (songId, accessToken) => {

        const res = await fetch(`tracker/add_to_playlist`, {        // add autoplaylist functionality
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`
            },
            body: JSON.stringify({
                songId,
                accessToken
            })
        });

        if (res.status == 201) {
            return true;
        }

        if (res.status == 401) {
            return false;
        }

        else if (res.status == 403) {

            console.log("playlist size exceeded");
            return false;
        }
        return false;
    },

    getPlaylist: async (accessToken) => {
        const res = await fetch(`tracker/get_playlist`, {        // add autoplaylist functionality
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`
            }
        });

        const parsedRes = await res.json();

        if (res.status == 200) {
            return { playlistId: parsedRes.playlistId }
        }

        return { responseType: "error", description: "error fetching playlist" }
    },

    getSongInformation: async (room, accessToken, songId) => {
        const res = await fetch(`https://api.spotify.com/v1/tracks/${songId}`, {        // add autoplaylist functionality
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`
            }
        });

        if (res.status != 200) {
            return {error: "song_not_found", description: "search song couldn't be found"}
        }

        const parsedRes = await res.json();

        return {room, roomInfo: parsedRes};
    }
}

export default Tracker;