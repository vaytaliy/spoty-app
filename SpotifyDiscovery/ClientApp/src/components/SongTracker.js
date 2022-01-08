import React, { useState, useEffect } from 'react';
import Tracker from './logic/Tracker';
import {SpotiPlayer} from './SpotiPlayer';
import Auth from './logic/Auth';


const SongTracker = (props) => {

    let autoskip = window.localStorage.getItem("autoSkip") || false;

    const [registerMessage, setRegisterMessage] = useState(null);
    const [autoskipIsEnabled, setAutoskipIsEnabled] = useState(autoskip);
    const [requestTimeoutIsCreated, setRequestTimeoutIsCreated] = useState(false);
    const [skipSong, setSkipSong] = useState(false);
    const [retryAttempts, setRetryAttempts] = useState(0);
    const [playlistId, setPlaylistId] = useState('');
    const [songId, setSongId] = useState(null);

    let timeout = null;
    //let playlistIdGet = Tracker.getPlaylistId(window.localStorage.getItem('access_token'));

    useEffect(() => {
        const playlistId = window.localStorage.getItem('playlist_id');

        if (playlistId === '') {
            setPlaylistId(`https://open.spotify.com/playlist/${playlistId}`);
        } else {
            getPlaylistId()
        }
    }, [])

    const getPlaylistId = async () => {

        const accessToken = window.localStorage.getItem("access_token");
        const playlistResult = await Tracker.getPlaylist(accessToken);
        console.log("playlist result", playlistResult)


        if (playlistResult && playlistResult.playlistId) {
            setPlaylistId(playlistResult.playlistId)
            return;
        }
        setPlaylistId('')
    }

    const setCurrentSongId = async (playbackData) => {

        const currentSong = songId;

        if (currentSong != songId) {
            await registerSong(playbackData);
        }
    }

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

    const saveSongToPlaylist = async () => {

        let accessToken = window.localStorage.getItem("access_token");

        if (songId) {
            console.log("access token", accessToken)
            await Tracker.saveSongToPlaylistRequest(songId, accessToken);
        }
    }

    const processSongRegistration = async (requestData, accessToken) => {

        let res = await Tracker.handleSongRegistrationRequest(requestData, accessToken)

        if (res == "failure" && retryAttempts < 5) {
            setRetryAttempts(retryAttempts += 1);
        } else if (res == "failure" && retryAttempts >= 5) {
            res.result = "retry_error";
        }

        renderSuccessResultMessage(res.result);
        setSkipSong(false);
        setRequestTimeoutIsCreated(false);
    }

    const registerSong = async (requestData, autoPlaylistURL = null) => {

        setRegisterMessage({ status: "wait", message: "Checking the song in the system.." });
        const accessToken = window.localStorage.getItem("access_token");

        console.log('registering song')

        clearTimeout(timeout);

        if (!requestTimeoutIsCreated) {

            setRequestTimeoutIsCreated(true);
            timeout = setTimeout(async () => await processSongRegistration(requestData, accessToken), 2000) // to give some delay on song processing and unload server
        }
    }

    const drawPlaylist = () => {

        if (playlistId !== '') {
            return (
                <div>
                    <a href={`https://open.spotify.com/playlist/${playlistId}`}>See playlist</a>
                </div>
            );
        } else {
            return (<div>
                Something went wrong and playlist is not available
            </div>
            );
        }
    }

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
            {drawPlaylist()}
            <button onClick={saveSongToPlaylist}>Save to playlist</button>
            <SpotiPlayer data={props.data} playerControl={{ skipSong }} tracking={{ registerSong, skipSong, autoskipIsEnabled, saveSongToPlaylist, setCurrentSongId, songId, setSongId }} />
        </div>
    );
}

export default SongTracker;