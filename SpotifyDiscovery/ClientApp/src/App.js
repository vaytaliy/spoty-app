import React, { useState, useEffect } from 'react';
import { Redirect, Route, Switch, Router } from 'react-router-dom';
import { createBrowserHistory } from 'history'
//import { Layout } from './components/Layout';
import Home from './components/Home';
import Landing from './components/Landing';
import CallbackComponent from './components/CallbackComponent';

import './custom.css'
import Login from './components/Login';
import AuthLogic from './components/logic/Auth';
import SharedPlayer from './components/SharedPlayer';
import Hosting from './components/logic/Hosting';
//import connection from './components/logic/Realtime'

let history = createBrowserHistory();


const App = () => {


	// Start the connection.
	console.log(process.env.REACT_APP_HOST_URL)
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

	useEffect(() => {
		(async () => {
			await Hosting.initSignalR()
		})();
	}, []);

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
		}
	}

	return (
		<div>
			<Route exact path='/'> <Landing /></Route>
			<Route path='/home'>
				{isLoggedIn ? <Home data={{ setRefreshedTokens, isLoggedIn, credentials, runRefreshAuthorization, authenticationFailed, modifyLoginState }} /> : <Redirect to='/unauthorized' />}
			</Route>
			{/* <Route path='/unauthorized' data={{ isLoggedIn }} render={(isLoggedIn) => {
				if (!isLoggedIn) {
					return <Login />
				}
			}}>
			</Route> */}
			<Route path='/unauthorized'> <Login />
			</Route>
			<Route path='/callback'>
				<CallbackComponent data={{ modifyLoginState }} />
			</Route>
			<Route path='/rooms/:id'>
				{isLoggedIn ? <SharedPlayer playerControl={{ skipSong: null }} history={history}
					data={{
						setRefreshedTokens,
						isLoggedIn,
						credentials,
						runRefreshAuthorization,
						authenticationFailed,
						modifyLoginState
					}} /> : <Redirect to='/unauthorized' />}
			</Route>
			{/* <Route path='/join/:id'>
				<Switch>
					<SharedPlayer playerControl={{ skipSong: null }}
						data={{
							setRefreshedTokens,
							isLoggedIn,
							credentials,
							runRefreshAuthorization,
							authenticationFailed,
							modifyLoginState
						}} action="join" />
				</Switch>
			</Route> */}
		</div>
	);
}

export default App;