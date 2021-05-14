import {trackedMusicThisSession} from '../storage/InMemoryStorage';

const Tracker = {

    handleSongRegistrationRequest: async (requestData) => {
        if (!requestData.songId) {
            return {result: "failure"}
        }
        //requestData.accessToken
        if (trackedMusicThisSession.has(requestData.songId)){
            
            console.log("already in memory");
            return {result: "in_memory"};
        }

        const res = await fetch('https://localhost:44370/tracker/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                accountId: requestData.accountId,
                songId: requestData.songId
            })
        });

        if (res.status != 200) {
            return {result: "failure"};
        }

        trackedMusicThisSession.add(requestData.songId);
        const parsedRes = await res.json();
        
        return parsedRes;
    },


    //TODO part of functionality

        // const pushSongToAutoPlaylist = (songId, accessToken, autoPlaylistId) => {

    //     const res = await fetch('https://localhost:44370/tracker/register', {        // add autoplaylist functionality
    //         method: 'POST',
    //         headers: {
    //             'Content-Type': 'application/json',
    //         },
    //         body: JSON.stringify({
    //             ...requestData.accountId,
    //             ...requestData.songId
    //         })
    //     });

    // }

    //playlist id can be taken from URL
    // if person listens to music in fresh playlist, it wont be removed from there. It's up to user to do so

}

export default Tracker;