import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';

const Login = (props) => {

    let navigate = useNavigate();

    useEffect(() => {
        if (props.userRedirected == false) {
            navigate("/")
        }
    }, [])

    return (
        <div>
            <h2>You must log in to spotify</h2>
            <a href="/discovery/login">Log in to spotify</a>
        </div>
    );
}

export default Login;