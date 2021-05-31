import React, { useState, useEffect } from 'react';
import { Route, Redirect } from 'react-router';
import {useParams} from 'react-router-dom';
import AuthLogic from './logic/Auth';

// this component
// only handles saving tokens into local storage
// and then redirecting user to Home page

const CallbackComponent = (props) => {

    let isSuccessful = false;
    const urlParams = new URLSearchParams(window.location.search);
        
    let accessToken = urlParams.get('access_token');
    let refreshToken = urlParams.get('refresh_token');
    let playlistId = urlParams.get('playlist');

    isSuccessful = AuthLogic.saveCreds(accessToken, refreshToken, playlistId)

    if (isSuccessful){

        props.data.modifyLoginState(true)
        return <Redirect to='/home' />
    }
    return <Redirect to='/unauthorized?error=unable+to+save+credentials' />
};

export default CallbackComponent;
