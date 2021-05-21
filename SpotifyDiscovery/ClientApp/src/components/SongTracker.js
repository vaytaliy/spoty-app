import React, { useState, useEffect } from 'react';
import Tracker from './logic/Tracker';
import SpotiPlayer from './SpotiPlayer';
import Auth from './logic/Auth';

const SongTracker = (props) => {

    let autoskip = window.localStorage.getItem("autoSkip") || false;

    const [registerMessage, setRegisterMessage] = useState(null);
    const [autoskipIsEnabled, setAutoskipIsEnabled] = useState(autoskip);
    const [requestTimeoutIsCreated, setRequestTimeoutIsCreated] = useState(false);
    const [skipSong, setSkipSong] = useState(false);
    const [retryAttempts, setRetryAttempts] = useState(0);

    let timeout = null;

    const renderSuccessResultMessage = (resultMessage) => {

        switch (resultMessage) {
            case "song_exists":
                setRegisterMessage({ status: "old", message: "You've already heard this one" });
                setSkipSong(true);
                break;
            case "new_song":
                setRegisterMessage({ status: "new", message: "Successfully registered new song" });
                setSkipSong(false);
                break;
            case "new_id":
                setRegisterMessage({ status: "new", message: "Registered your first song" });
                setSkipSong(false);
                break;
            case "retry_error":
                setRegisterMessage({ status: "new", message: "Couldn't process song. Too many retry attempts :(" });
                setSkipSong(false);
                break;
            case "in_memory":
                setRegisterMessage({ status: "old", message: "Heard this in current session, perhaps the playlist is repeating.." });
                setSkipSong(true);
                break;
            default:
                setSkipSong(false);
                break;
        }
    }

    const handleAutoskipChange = () => {
        const currentState = autoskipIsEnabled;
        setAutoskipIsEnabled(!currentState);
    }

    const saveSongToPlaylist = async (songId) => {

        let accessToken = window.localStorage.getItem("access_token");
        let playlistId = window.localStorage.getItem("playlist");

        await Tracker.saveSongToPlaylistRequest(songId, accessToken, playlistId);
    }

    //requestData contains such:
    // accessToken
    // accountId (nullable)
    // songId
    const processSongRegistration = async (requestData) => {
        
        let res = await Tracker.handleSongRegistrationRequest(requestData)

        if (res == "failure" && retryAttempts < 5) {
            setRetryAttempts(retryAttempts += 1);
            registerSong();
        } else if (res == "failure" && retryAttempts >= 5) {
            res.result = "retry_error";
        }
        // } else if (res == "failure" && requestRetries >= 5) {
        //     processingStatus = "retry_error";
        // } else if (res == "in_memory") {
        //     processingStatus = "in_memory"
        // } else if (res && res.result) {  // has result
        //     processingStatus = res.result;
        // }

        // TODO insert auto playlist logic
        renderSuccessResultMessage(res.result);
        setSkipSong(false);
        setRequestTimeoutIsCreated(false);
    }

    const registerSong = async (requestData, autoPlaylistURL = null) => {

        setRegisterMessage({ status: "wait", message: "Checking the song in the system.." });

        let requestedUser = await Auth.requestAccountId(requestData.accessToken) //

        if (requestedUser.errorMessage) {

            console.log("error getting user")
            return; //TODO Handle that error, it is authorization error
        }

        requestData.accountId = requestedUser.id; // set to found id
        clearTimeout(timeout);

        if (!requestTimeoutIsCreated) {

            setRequestTimeoutIsCreated(true);
            timeout = setTimeout(async () => await processSongRegistration(requestData), 2000) // to give some delay on song processing and unload server
        }
    }
    
    // useEffect(() => {
        
    //     console.log("getting new tokens")
    //     //testing new tokens
    //     setTimeout(async() => {
    //         await props.data.setRefreshedTokens()
    //     }, 20 * 1000)

    // }, [])
    

    return (
        <div>
            Song tracker
            {registerMessage && registerMessage.message ? <h3>{registerMessage.message}</h3> : ''}
            <label>
                Enable autoskip old music:
                <input
                    type="checkbox"
                    name="autoskip"
                    checked={autoskipIsEnabled}
                    onChange={handleAutoskipChange}
                />
            </label>
            <SpotiPlayer data={props.data} tracking={{ registerSong, skipSong, autoskipIsEnabled, saveSongToPlaylist }} />
        </div>
    );
}

export default SongTracker;