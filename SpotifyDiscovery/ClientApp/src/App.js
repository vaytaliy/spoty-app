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
	const [loginAttemptSuccess, loginAttemptFail] = useState(false);

	let success = false;
	const accessToken = window.localStorage.getItem('access_token');
	console.log(accessToken);

	const modifyLoginState = (loginIsSuccessful) => {

		console.log('the login state is set to', loginIsSuccessful);
		setIsLoggedIn(loginIsSuccessful);
	}

	const setRefreshedTokens = async () => {

		const res = await AuthLogic.tryRefreshTokens();

		if (res.successResult) {

			console.log("renewed the credentials")
			setIsLoggedIn(true);

		} else if (res.error) {

			console.log("couldn't renew credentials")
			setIsLoggedIn(false);
			loginAttemptFail(true);
		}
	}

	// if (!accessToken){
	// 	success = false
	// 	//setIsLoggedIn(false);
	// } else {
	// 	success = true;
	// 	//setIsLoggedIn(true);
	// }

	// useEffect(() => {
	// 	if (!isLoggedIn){
	// 		success = false;
	// 	}
	// }, [isLoggedIn])

	return (
		<div>
			<Route path='/home'>
				{loginAttemptFail ? <Home data={{ modifyLoginState, setRefreshedTokens }} /> : <Redirect to='/unauthorized' />}
			</Route>
			<Route path='/unauthorized'>
				<Login />
			</Route>
			<Route path='/callback'>
				<CallbackComponent data={{ modifyLoginState }} />
			</Route>
		</div>
		// <Router>
		// 	<Route exact path='/' component={Landing} />
		// 	<Route path='/main' component={Home} />
		// 	<Route path='/unauthorized' component={Login} />
		// 	<Route path='/callback' component={CallbackComponent} />
		// </Router>
	);
}

export default App;