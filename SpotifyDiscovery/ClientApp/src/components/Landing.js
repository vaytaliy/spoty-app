import React, { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import Home from './Home';
import Tracker from './logic/Tracker';

const Landing = (props) => {

    const [activeRooms, setActiveRooms] = useState([]);
    const [searchParams, setSearchParams] = useSearchParams();
    const [errorDisplay, setErrorDisplay] = useState(null);


    useEffect(() => {
        getActiveRooms()
    }, []);

    const getActiveRooms = async () => {

        let abortController = new AbortController();
        let page = searchParams.get('page');

        if (page == null) {
            page = 1;
        }

        const res = await fetch(`room_api/active_rooms?page=${page}`, {
            method: 'GET',
            signal: abortController.signal,
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (abortController.signal.aborted) {
            abortController.abort();
            return { error: "aborted", description: "request operation was aborted" };
        }

        if (res.status != 200) {
            return { error: "unexpected_error", description: "unexpected error occured" };
        }

        const resData = await res.json();

        if (resData && resData.error) {
            setErrorDisplay(() => ({
                error: resData.error,
                description: resData.description
            }));
            return;
        }
        if (resData.length > 0) {
            handleReceivedRooms(resData);
        }
    };

    const handleReceivedRooms = async (rooms) => {

        const accessToken = window.localStorage.getItem("access_token");
        const roomInfoPromises = []
        let activeRoomsList = []

        for (const room of rooms) {
            const promise = Tracker.getSongInformation(room, accessToken, room.activeSong)
            roomInfoPromises.push(promise)
        }

        var roomInfoResult = await Promise.all(roomInfoPromises)

        roomInfoResult.forEach((result, index) => {
            const link = `rooms/${result.room.ownerId}`
            activeRoomsList.push(
                <React.Fragment key={index}>
                    <div className="ui card">
                        <h3>Host: {result.room.ownerId}</h3>
                        <div className="image">
                            <img src={result.roomInfo.album.images[2].url} />
                        </div>
                        <div>Currently listening to: {result.roomInfo.album.name}</div>
                        <a href={link}>Join room</a>
                    </div>
                </React.Fragment>)
        })
        setActiveRooms(activeRoomsList)
    };

    return (
        <div>
            This is the main page
            <div>
                <a href="/home">Send me to the music tracker</a>
            </div>
            <Home renderPlayer={false} data={props.data} />
            {errorDisplay != null ? (
                <div>
                    <h2>{errorDisplay.error}</h2>
                    <div>{errorDisplay.description}</div>
                </div>) : null}
            {activeRooms}
        </div>
    );
}

export default Landing;