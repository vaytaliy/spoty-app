import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';

const Login = () => {

    useEffect(() => {
        console.log("rendered login page")
    }, [])

    return (
        <div>
            <h2>You must log in to spotify</h2>
            <a href="/discovery/login">Log in to spotify</a>
        </div>
    );
}

export default Login;