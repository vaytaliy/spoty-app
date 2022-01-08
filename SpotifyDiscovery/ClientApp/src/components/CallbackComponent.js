import React, { useState, useEffect } from 'react';
import { Navigate, useNavigate } from 'react-router-dom';
import AuthLogic from './logic/Auth';

// this component
// only handles saving tokens into local storage
// and then redirecting user to Home page

let successResult = {redirectLink: "/"}

const CallbackComponent = (props) => {

    //const navigate = useNavigate();
    let navigate = useNavigate();
    

    const getCredsAndRedirect = () => {
        let isSuccessful = false;

        const urlParams = new URLSearchParams(window.location.search);
        
        let accessToken = urlParams.get('access_token');
        let refreshToken = urlParams.get('refresh_token');
    
        isSuccessful = AuthLogic.saveCreds(accessToken, refreshToken)
    
        if (isSuccessful){
            navigate(-2);
        }
        props.data.modifyLoginState(isSuccessful) 
    }

    useEffect(() => {
        getCredsAndRedirect()
    }, [])

    return (
        <div>
            <p>Authorizing..</p>
        </div>
    );
};

export {CallbackComponent, successResult};
