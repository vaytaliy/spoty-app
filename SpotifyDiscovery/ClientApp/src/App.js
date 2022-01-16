import React, { useState, useEffect } from 'react';
import { Route, BrowserRouter, Routes, useNavigate, useLocation } from 'react-router-dom';
import { createBrowserHistory } from 'history'
//import { Layout } from './components/Layout';
import Home from './components/Home';
import Landing from './components/Landing';
import {CallbackComponent, successResult} from './components/CallbackComponent';

import './custom.css'
import Login from './components/Login';
import AuthLogic from './components/logic/Auth';
import SharedPlayer from './components/SharedPlayer';
import Hosting from './components/logic/Hosting';
//import connection from './components/logic/Realtime'

let history = createBrowserHistory();
//Hosting.initSignalR(window.localStorage.getItem('access_token'))

const App = () => {

	// Start the connection.
	let navigate = useNavigate();
	const [isLoggedIn, setIsLoggedIn] = useState(true);
	const [authRefreshTimerIsCreated, setAuthRefreshTimerIsCreated] = useState(false);
	const [credentials, setCredentials] = useState(
		{
			accessToken: window.localStorage.getItem('access_token') || null,
			refreshToken: window.localStorage.getItem('refresh_token') || null
		}
	);
	const [authenticationFailed, setAuthenticationFailed] = useState(false);
	let authRefreshTimeout = null;


	// useEffect(() => {
	// 	(async () => {
	// 		await Hosting.initSignalR(window.localStorage.getItem('access_token'))
	// 	})();
	// }, []);

	// useEffect(() => {	//uncomment this if want to reauth every 60 mins

	// 	clearTimeout(authRefreshTimeout);

	// 	if (!authRefreshTimerIsCreated) {
	// 		setAuthRefreshTimerIsCreated(true);
	// 		authRefreshTimeout = setTimeout(runRefreshAuthorization, 15 * 1000);
	// 	}

	// }, [authRefreshTimerIsCreated])

	const runRefreshAuthorization = () => {
		console.log('making reauth')
		setRefreshedTokens();
		setAuthRefreshTimerIsCreated(false);
	}

	const modifyLoginState = (loginIsSuccessful) => {
		console.log('the login state is set to', loginIsSuccessful);
		setIsLoggedIn(loginIsSuccessful);
		setAuthenticationFailed(false);
	}

	const setRefreshedTokens = async () => {

		const res = await AuthLogic.tryRefreshTokens();

		if (res && res.successResult) {

			setCredentials(res.credentials)
			console.log("renewed the credentials")
			setIsLoggedIn(true);
			setAuthenticationFailed(false);

		} else if (!res || res.error) {
			setIsLoggedIn(true);
			console.log("couldn't renew credentials")
			setIsLoggedIn(false);
			setAuthenticationFailed(true);
			navigate('/unauthorized')
		}
	}

	return (
			<Routes>
				<Route exact path='/' element={<Landing renderPlayer={false} data={{setRefreshedTokens, isLoggedIn, credentials, runRefreshAuthorization, authenticationFailed, modifyLoginState}}/>} />
				<Route path='/home'
					element={<Home renderPlayer={true} data={{setRefreshedTokens, isLoggedIn, credentials, runRefreshAuthorization, authenticationFailed, modifyLoginState}} />}
				/>
				<Route path='/unauthorized' element={<Login userRedirected={authenticationFailed} />} />
				<Route path='/callback' element={<CallbackComponent data={{modifyLoginState}} />} />
				{isLoggedIn ? <Route path='/rooms/:id' element={<SharedPlayer playerControl={{ skipSong: null }} history={history}
					data={{
						setRefreshedTokens,
						isLoggedIn,
						credentials,
						runRefreshAuthorization,
						authenticationFailed,
						modifyLoginState
					}} />} /> : null}
			</Routes>
	);
}

export default App;