import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import Hosting from './logic/Hosting';
import {SpotiPlayer} from './SpotiPlayer';
import AuthLogic from './logic/Auth';
import ConnectedUsers from './shared_player_components/ConnectedUsers';
import Chat from './shared_player_components/Chat';
import RoomError from './shared_player_components/RoomError';
import SettingsBox from './shared_player_components/SettingsBox';
import RoomLogin from './shared_player_components/RoomLogin';


const SharedPlayer = (props) => {

    const { id } = useParams();

    const [isHost, setIsHost] = useState(false);
    const [chatContents, setChatContents] = useState([]);

    const [displayLoginPrompt, setDisplayLoginPrompt] = useState(false);
    const [roomError, setRoomError] = useState({ errorType: null, errorDescription: null }); //use this to render prompt/spotiplayer
    //const [oldPassword, setOldPassword] = useState("");
    const [connectedPeople, setConnectedPeople] = useState(new Map());
    const [roomSettings, setRoomSettings] = useState({})
    const [loadPlayer, setLoadPlayer] = useState(false)
    const [isLoggedIn, setIsLoggedIn] = useState(false)
    const [deviceId, setDeviceId] = useState("")

    // useEffect(() => { 
    //     connectToRoom()
    // }, [])

    useEffect(() => {
        connectToRoom();
    }, [props.data.credentials])

    useEffect(() => {
        console.log(props.data.isLoggedIn)
        
    }, [props.data.isLoggedIn])

    const handleSetConnectedPeople = (connectedPeopleMap) => { //invoke m
        setConnectedPeople(connectedPeopleMap);
    };

    const setPlayerDeviceId = (deviceId) => {
        Hosting.deviceId = deviceId;
        setDeviceId(deviceId);
    }

    const handleReceiveChatMessage = async (incomingMessage, spotifyIdSender) => {
        const account = await AuthLogic.requestAccountId(window.localStorage.getItem('access_token'));

        if (account) {
            if (spotifyIdSender === account.id) {
                incomingMessage.thisUser = true
            } else if (spotifyIdSender !== account.id) {
                incomingMessage.thisUser = false
            }

            setChatContents(oldChatContents => [...oldChatContents, incomingMessage]); //invoke at Chat component
        }
    };

    const handleOnloadRoomHostOptions = (roomOptions) => { //TODO invoke at child "SettingsBox"
        setRoomSettings(() => ({
            ...roomOptions
        }))
    };

    const getRoomRequirements = async () => {
        const accessToken = window.localStorage.getItem("access_token");

        let roomRequirementsRawResponse = await fetch(`room_api/${id}/settings`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${accessToken}`
            }
        });

        const roomRequirements = await roomRequirementsRawResponse.json();

        if (roomRequirements.error){
            setRoomError(() => ({
                errorType: roomRequirements.error,
                errorDescription: roomRequirements.description
            }))
        }

        if (roomRequirements.passwordRequired) {
            setDisplayLoginPrompt(true);
        } else {
            await connect()
        }
    }

    const handleRoomLoginRequired = (isRequired) => {
        setDisplayLoginPrompt(isRequired);
    };

    const handleRoomError = (errorPayload) => {
        setRoomError((prevState) => ({
            errorType: errorPayload.error,
            errorDescription: errorPayload.description
        }))

        if (errorPayload.error === "friendlist_error") {

        }
    }

    const initIsHost = async (isHost) => {
        setRoomError({})
        if (isHost === false) {
            setDisplayLoginPrompt(false);
            
        }
        setIsHost(isHost)
        setIsLoggedIn(true)
    };

    const changeRoomProperties = async (properties) => {
        await Hosting.changeRoomProperties(properties)
    };

    const connect = async (joinPassword) => {
        try {
            await Hosting.connectToRoom(id,
                {
                    handleSetConnectedPeople,
                    handleReceiveChatMessage,
                    handleOnloadRoomHostOptions,
                    handleRoomLoginRequired,
                    handleRoomError,
                    initIsHost,
                    runRefreshAuthorization: props.data.runRefreshAuthorization,
                    credentials: props.data.credentials
                },
                joinPassword
            );
            setLoadPlayer(true);
        } catch (err) {
            setRoomError({})
        }
    }

    const connectToRoom = async (joinPassword = "") => {

        await Hosting.initSignalR("")
        

        const accessToken = window.localStorage.getItem('access_token');
        const account = await AuthLogic.requestAccountId(accessToken);
        const roomId = id;

        if (account.id !== roomId) {
            await getRoomRequirements()
        } else {
            await connect(joinPassword);
        }
    }

    return (
        <div>
            <RoomError data={{roomError}}/>
            {isHost ? <SettingsBox methods={{changeRoomProperties}} data={{roomSettings}}/> : null }
            {displayLoginPrompt ? <RoomLogin methods={{connect}}/> : null}
            {isLoggedIn ? <SpotiPlayer data={props.data} playerControl={props.playerControl} windowmanage={setPlayerDeviceId} sharing={true} listeningTo={id} /> : null}
            {isLoggedIn ? <ConnectedUsers data={{connectedPeople, id}}/> : null}
            {isLoggedIn ? <Chat data={{chatContents}}/> : null}
        </div>
    );
}

export default SharedPlayer