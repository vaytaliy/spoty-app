import {SpotiPlayer} from './SpotiPlayer';
import React, { useState, useEffect } from 'react';
import SongTracker from './SongTracker';
import AppInfo from '../constants';
import AuthLogic from './logic/Auth';

const Home = (props) => {

    const [hostId, setHostId] = useState('')

    useEffect(() => {
        getAccountId()
    }, [])

    const getAccountId = async () => {
        const userData = await AuthLogic.requestAccountId(window.localStorage.getItem('access_token'));

        if (userData.errorMessage) {
            setHostId('')
            console.log(props)
            props.data.runRefreshAuthorization()
        } else {
            setHostId(userData.id)
        }
    }

    const renderHosting = () => {
        if (hostId !== '') {
            return (<a href={`rooms/${hostId}`}>Start hosting</a>)
        }
    }

    return (
        <div>
            <SongTracker data={props.data} />
            {renderHosting()}
        </div>
    );
}

export default Home