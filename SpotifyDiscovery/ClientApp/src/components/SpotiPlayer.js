import React, { useState, useEffect, useLayoutEffect } from 'react';
import { Redirect } from 'react-router';
import WebPlayer from './logic/WebPlayer';
import AuthLogic from './logic/Auth';
import Hosting from './logic/Hosting';
import Skeleton from '@mui/material/Skeleton'

let spotyPlayer = null;
let deviceId = null;

const initPlayerData = () => {
    return {
        previousSongId: '',
        currentSongId: '',
        songPlayCount: 0,
        tokenCreationTime: null,
        tokenScheduledRefresh: null,
        onSongChange: (newSongId) => {
            console.log('track change', newSongId);
        }
    }
}

let playerData = initPlayerData();

const SpotiPlayer = (props) => {

    const [loadMessage, setLoadMessage] = useState(null);
    const [songId, setSongId] = useState(null);
    const [timerIsCreated, setTimerIsCreated] = useState(false);
    const [trackIsPlaying, setTrackIsPlaying] = useState(false);
    const [playerState, setPlayerState] = useState(null);
    const [currentSong, setCurrentSong] = useState(null);
    const [errorMessage, setErrorMessage] = useState(null); //needs to be sent a level higher

    let timeout = null;

    useEffect(() => {
        const spotyPlayerScript = document.createElement('script');
        spotyPlayerScript.src = "https://sdk.scdn.co/spotify-player.js";
        spotyPlayerScript.async = true;
        document.body.appendChild(spotyPlayerScript);

        return () => {
            document.body.removeChild(spotyPlayerScript);
        }
    }, [])

    useEffect(() => {
        startSpotifySDK();
    }, [props.data.credentials])

    useEffect(() => {

        clearTimeout(timeout)

        if (spotyPlayer && props.tracking && props.playerControl.skipSong && props.tracking.autoskipIsEnabled && !timerIsCreated) {
            setTimerIsCreated(true)
            timeout = setTimeout(handleSkip, 3000);
        }

        function handleSkip() {
            spotyPlayer.nextTrack();
            setTimerIsCreated(false)
        }

    }, [props.playerControl.skipSong])

    const togglePlayback = () => {
        console.log(spotyPlayer)
        if (spotyPlayer) {
            spotyPlayer.togglePlay()
                .then(() => {
                    console.log('toggled playback')
                })
        }
    }

    const handlePlayPreviousTrack = () => {

        if (spotyPlayer) {
            spotyPlayer.previousTrack().then(() => {
                console.log('Set to previous track!');
            });
        }
    }

    const handlePlayNextTrack = () => {

        if (spotyPlayer) {
            spotyPlayer.nextTrack().then(() => {
                console.log('Set to next track!');
            });
        }
    }

    const initializePlayer = () => {
        let accessToken = window.localStorage.getItem("access_token");

        const player = new window.Spotify.Player({
            name: 'Spoty Discovery',
            getOAuthToken: tokenCallback => {
                tokenCallback(accessToken)
            },
            volume: 0.1
        });
        return player;
    }


    const addListeners = async () => {
        spotyPlayer.addListener('initialization_error', ({ message }) => { console.error(message); });

        spotyPlayer.addListener('account_error', ({ message }) => { console.error(message); });

        spotyPlayer.addListener('playback_error', async ({ message }) => {
            console.error(message);

            if (deviceId) {
                await WebPlayer.transferUserPlaybackHere(
                    deviceId,
                    window.localStorage.getItem('access_token'),
                    true);
            }
        });

        spotyPlayer.addListener('authentication_error', async state => {
            if (state.message !== "Browser prevented autoplay due to lack of interaction") {
                await props.data.setRefreshedTokens(); // this is good
                props.data.runRefreshAuthorization();
            }
            console.log(state);

        })

        spotyPlayer.addListener('player_state_changed', async state => {
            let newSong = WebPlayer.handleStateChange(state, playerData.currentSongId);
            //may return new song object or null if current song isn't new
            console.log(state);
            setPlayerState(state)
            if (newSong) {
                playerData.currentSongId = newSong.id
                playerData.previousSongId = newSong.oldSongId;
                playerData.state = state;
                playerData.onSongChange(playerData.currentSongId);

                if (props.tracking) {
                    props.tracking.setSongId(newSong.id);
                    await props.tracking.registerSong({ songId: newSong.id, accessToken: window.localStorage.getItem("access_token") });
                }
                //if this is a host and shares player, send update to others
                if (props.sharing) {
                    console.log(newSong.id)
                    await Hosting.updateState(newSong.id);
                }
                setCurrentSong(newSong)
            }
        });

        spotyPlayer.addListener('ready', async ({ device_id }) => {
            setLoadMessage(null)
            setErrorMessage(null)
            console.log('Ready with Device ID', device_id);
            deviceId = device_id;
            let playerIsPaused = false;

            if (playerState && playerState.paused) {
                playerIsPaused = playerState.paused
            }

            WebPlayer.transferUserPlaybackHere(
                device_id,
                window.localStorage.getItem('access_token'),
                !playerIsPaused);

            if (props.sharing) {

                const idOfGenericRoom = props.listeningTo //if not intended for hosting, id from pasted url
                let roomId
                const accessToken = window.localStorage.getItem('access_token');

                if (idOfGenericRoom) {
                    roomId = idOfGenericRoom;
                } else {
                    roomId = await AuthLogic.requestAccountId(accessToken);
                }

                try {
                    await Hosting.connectToRoom(roomId, accessToken, props.uiControls, spotyPlayer, deviceId);
                } catch (err) {
                    setErrorMessage(err.message)
                }
            }
        });

        spotyPlayer.addListener('not_ready', ({ device_id }) => {
            console.log('Device ID has gone offline', device_id);
        });
    }

    const reinitializePlayer = async () => {

        if (spotyPlayer) {
            spotyPlayer.removeListener('player_state_changed')
            spotyPlayer.removeListener('initialization_error')
            spotyPlayer.removeListener('account_error')
            spotyPlayer.removeListener('playback_error')
            spotyPlayer.removeListener('authentication_error')
            spotyPlayer.removeListener('player_state_changed')
            spotyPlayer.removeListener('ready')
            spotyPlayer.removeListener('not_ready')
            spotyPlayer.disconnect();
        }

        spotyPlayer = initializePlayer();
        addListeners();
        spotyPlayer.connect();
    }

    const startSpotifySDK = () => {

        if (window.Spotify) {
            reinitializePlayer();
        }

        window.onSpotifyWebPlaybackSDKReady = async () => {

            if (!spotyPlayer) {
                spotyPlayer = initializePlayer();
                addListeners();
                spotyPlayer.connect();
            }
        }
    }

    if (props.data && props.data.autenticationFailed) {
        return <Redirect to='/unauthorized' />
    }

    if (currentSong) {
        return (
            <div>
                <h2>{currentSong ? currentSong.name : 'none'}</h2>
                <img src={currentSong ? currentSong.imageURL : 'none'} />
                <p>{loadMessage}</p>
                <script src="https://sdk.scdn.co/spotify-player.js"></script>
                <p>This is spotify player component</p>
                <div>
                    <button onClick={togglePlayback}>{playerState && playerState.paused ? "play" : "pause"}</button>
                    <button onClick={handlePlayPreviousTrack}>Previous</button>
                    <button onClick={handlePlayNextTrack}>Next</button>
                </div>
                {errorMessage ? <div>{errorMessage}</div> : ''}
            </div>
        );
    } else {
        return (
            <Skeleton variant="rectangular" width={210} height={118} />
        );
    }

}

export default SpotiPlayer;