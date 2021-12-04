import { trackedMusicThisSession } from '../storage/InMemoryStorage';
import AppInfo from '../../constants';

const Tracker = {

    handleSongRegistrationRequest: async (requestData, accessToken) => {

        if (trackedMusicThisSession.has(requestData.songId)) {

            console.log("already in memory");
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

        console.log("ok")
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
            console.log("parsed res", parsedRes)
            return { playlistId: parsedRes.playlistId }
        }

        return { responseType: "error", description: "error fetching playlist" }
    }
}

export default Tracker;