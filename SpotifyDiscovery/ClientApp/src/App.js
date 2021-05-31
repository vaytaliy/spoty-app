import React, { useState, useEffect } from 'react';
import { Redirect, Route } from 'react-router-dom';
//import { Layout } from './components/Layout';
import Home from './components/Home';
import Landing from './components/Landing';
import CallbackComponent from './components/CallbackComponent';

import './custom.css'
import Login from './components/Login';
import AuthLogic from './components/logic/Auth';



const App = () => {

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

	// useEffect(() => {	//remove this if i explicitly want to reauth every 60 mins

	// 	clearTimeout(authRefreshTimeout);

	// 	if (!authRefreshTimerIsCreated) {
	// 		setAuthRefreshTimerIsCreated(true);
	// 		authRefreshTimeout = setTimeout(runRefreshAuthorization, 60 * 60 * 1000);
	// 	}

	// }, [authRefreshTimerIsCreated])


	const runRefreshAuthorization = () => {
		console.log('making reauth')
		setRefreshedTokens();
		setAuthRefreshTimerIsCreated(false);
	}

	const accessToken = window.localStorage.getItem('access_token');

	// const modifyLoginState = (loginIsSuccessful) => {

	// 	console.log('the login state is set to', loginIsSuccessful);
	// 	setIsLoggedIn(loginIsSuccessful);
	// }

	const setRefreshedTokens = async () => {


		const res = await AuthLogic.tryRefreshTokens();

		if (res.successResult) {

			setCredentials(res.credentials)
			console.log("renewed the credentials")
			setIsLoggedIn(true);
			setAuthenticationFailed(false);
			//window.location.reload();

		} else if (res.error) {

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
				{isLoggedIn ? <Home data={{setRefreshedTokens, isLoggedIn, credentials, runRefreshAuthorization, authenticationFailed }} /> : <Redirect to='/unauthorized' />}
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
				<CallbackComponent />
			</Route>
		</div>
	);
}

export default App;