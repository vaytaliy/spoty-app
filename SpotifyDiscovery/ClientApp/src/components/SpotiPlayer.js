import React, { useState, useEffect } from 'react';
import WebPlayer from './logic/WebPlayer';

const SpotiPlayer = (props) => {

    const [loadMessage, setLoadMessage] = useState(null);
    //const [autoSkip, setAutoskip] = useState({ skipSeconds: 1000, isEnabled: false })
    const [currentSong, setCurrentSong] = useState(null);
    const [songId, setSongId] = useState(null);
    const [player, setPlayer] = useState(null); // initialized on spotify SDK
    const [timerIsCreated, setTimerIsCreated] = useState(false);
    const [trackIsPlaying, setTrackIsPlaying] = useState(false);

    let currentSongId = null;
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
    //props.modifyLoginState
    // useEffect(() => {

    //     console.log("skip?", props.tracking.skipSong, props.tracking.autoskipIsEnabled)
    //     const skipTrack = async () => {
    //         console.log(player);

    //         
    //     }
    //     skipTrack();
    // }, [songId])

    useEffect(() => {

        clearTimeout(timeout)

        if (player && props.tracking && props.tracking.skipSong && props.tracking.autoskipIsEnabled && !timerIsCreated) {
            setTimerIsCreated(true)
            timeout = setTimeout(handleSkip, 3000);
        }

        function handleSkip() {
            console.log('===')


            console.log(props.tracking.skipSong);
            console.log(props.tracking.autoskipIsEnabled);

            player.nextTrack();
            console.log("skipped track");
            setTimerIsCreated(false)
        }

        console.log('timeout', timeout);
    }, [props.tracking.skipSong])

    const togglePlayback = () => {

        if (player) {
            player.togglePlay()
                .then(() => {
                    console.log('toggled playback')
                })
        }
    }

    const handlePlayPreviousTrack = () => {

        if (player) {
            player.previousTrack().then(() => {
                console.log('Set to previous track!');
            });
        }
    }

    const handlePlayNextTrack = () => {

        if (player) {
            player.nextTrack().then(() => {
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

    window.onSpotifyWebPlaybackSDKReady = async () => {

        const player = initializePlayer();

        //playerSetState({player})
        player.addListener('player_state_changed', state => {
            if (state) {
                setTrackIsPlaying(!state.paused);
            }
        })

        player.addListener('initialization_error', ({ message }) => { console.error(message); });

        player.addListener('authentication_error', ({ message }) => { console.error(message); });

        player.addListener('account_error', ({ message }) => { console.error(message); });

        player.addListener('playback_error', ({ message }) => { console.error(message); });

        player.addListener('authentication_error', async state => {

            console.log('auth error');
            await props.data.setRefreshedTokens(); // this is good

        })

        player.addListener('player_state_changed', async state => {

            let newSong = WebPlayer.handleStateChange(state, currentSongId);
            //may return new song object or null if current song isn't new

            if (newSong) {
                currentSongId = newSong.id
                setSongId(currentSongId);

                if (props.tracking) {

                    const accessToken = window.localStorage.getItem("access_token");
                    console.log(newSong.id)

                    await props.tracking.registerSong({ songId: newSong.id, accessToken });
                }

                setCurrentSong(newSong)
            }
        });


        player.addListener('ready', async ({ device_id }) => {
            setLoadMessage(null)
            //await WebPlayer.setLatestPlayingTrack()
            setPlayer(player)
            console.log('Ready with Device ID', device_id);
            await props.data.modifyLoginState(true); // TODO:cross check this
        });

        player.addListener('not_ready', ({ device_id }) => {
            console.log('Device ID has gone offline', device_id);
        });

        player.connect(); //dont forget to uncomment to test connecting
    }

    return (
        <div>
            <h2>{currentSong ? currentSong.name : 'none'}</h2>
            <img src={currentSong ? currentSong.imageURL : 'none'} />
            <p>{loadMessage}</p>
            <script src="https://sdk.scdn.co/spotify-player.js"></script>
            <p>This is spotify player component</p>
            <div>
                <button onClick={togglePlayback}>{trackIsPlaying ? "pause" : "play"}</button>
                <button onClick={handlePlayPreviousTrack}>Previous</button>
                <button onClick={handlePlayNextTrack}>Next</button>
                <button onClick={() => props.tracking.saveSongToPlaylist(songId)}>Save it</button>
            </div>
        </div>
    );
}

export default SpotiPlayer;