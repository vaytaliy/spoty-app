import React, { useState, useEffect, useLayoutEffect } from 'react';
import { Redirect } from 'react-router';
import WebPlayer from './logic/WebPlayer';

let spotyPlayer = null;

const SpotiPlayer = (props) => {

    const [loadMessage, setLoadMessage] = useState(null);
    const [songId, setSongId] = useState(null);
    const [timerIsCreated, setTimerIsCreated] = useState(false);
    const [trackIsPlaying, setTrackIsPlaying] = useState(false);
    const [playerState, setPlayerState] = useState(null);
    const [currentSong, setCurrentSong] = useState(null);
    //const [currentSongId, setCurrentSongId] = useState(null);

    let currentSongId = null;
    let timeout = null;

    useEffect(() => {
        //initially this was used
        //for every new passed credentials
        const spotyPlayerScript = document.createElement('script');
        spotyPlayerScript.src = "https://sdk.scdn.co/spotify-player.js";
        spotyPlayerScript.async = true;
        document.body.appendChild(spotyPlayerScript);
        console.log(window.localStorage.getItem('access_token'));
        startSpotifySDK();
        return () => { // cleans up on rerender
            document.body.removeChild(spotyPlayerScript);
            console.log('cleanup');
            const childEls = document.body.children;

            let iframes = [];

            for (let i = 0; i < childEls.length; i++) {
                if (childEls[i].tagName === 'IFRAME') {
                    iframes.push(childEls[i]);
                }
            }

            for (let i = 0; i < iframes.length; i++) {
                iframes[i].remove();
            }

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
        }
    }, [props.data.credentials])

    useEffect(() => {

        clearTimeout(timeout)

        if (spotyPlayer && props.tracking && props.tracking.skipSong && props.tracking.autoskipIsEnabled && !timerIsCreated) {
            setTimerIsCreated(true)
            timeout = setTimeout(handleSkip, 3000);
        }

        function handleSkip() {
            spotyPlayer.nextTrack();
            setTimerIsCreated(false)
        }

    }, [props.tracking.skipSong])


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
        const accessToken = window.localStorage.getItem("access_token");

        const player = new window.Spotify.Player({
            name: 'Spoty Discovery',
            getOAuthToken: cb => {
                setLoadMessage('Connecting you with Spotify...')
                cb(accessToken);
            },
            volume: 0.3
        });
        return player;
    }

    const startSpotifySDK = () => {

        window.onSpotifyWebPlaybackSDKReady = async () => {

            spotyPlayer = initializePlayer();

            spotyPlayer.addListener('initialization_error', ({ message }) => { console.error(message); });

            spotyPlayer.addListener('account_error', ({ message }) => { console.error(message); });

            spotyPlayer.addListener('playback_error', ({ message }) => { console.error(message); });

            spotyPlayer.addListener('authentication_error', async state => {

                console.log('auth error');
                await props.data.setRefreshedTokens(); // this is good
                props.data.runRefreshAuthorization();
            })

            spotyPlayer.addListener('player_state_changed', async state => {

                let newSong = WebPlayer.handleStateChange(state, currentSongId);
                //may return new song object or null if current song isn't new

                if (newSong) {
                    currentSongId = newSong.id
                    //setSongId(currentSongId);
                    props.tracking.setSongId(newSong.id);
                    if (props.tracking) {

                        await props.tracking.registerSong({ songId: newSong.id, accessToken: window.localStorage.getItem("access_token") });
                    }

                    setCurrentSong(newSong)
                }
            });


            spotyPlayer.addListener('ready', async ({ device_id }) => {
                setLoadMessage(null)
                //await WebPlayer.setLatestPlayingTrack()

                console.log('Ready with Device ID', device_id);

                let playerIsPaused = false;

                if (playerState && playerState.state.paused) {
                    playerIsPaused = playerState.state.paused
                }
                await WebPlayer.transferUserPlaybackHere(
                    device_id,
                    window.localStorage.getItem('access_token'),
                    playerIsPaused);

                //await props.data.modifyLoginState(true); // TODO:cross check this
            });

            spotyPlayer.addListener('not_ready', ({ device_id }) => {
                console.log('Device ID has gone offline', device_id);
            });

            spotyPlayer.connect();
        }
    }

    if (props.data && props.data.autenticationFailed) {
        return <Redirect to='/unauthorized' />
    }
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
        </div>
    );

}

export default SpotiPlayer;