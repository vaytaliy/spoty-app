
const saveTokens = (accessToken, refreshToken) => {
    if (refreshToken) {
        window.localStorage.setItem('refresh_token', refreshToken);
    }

    if (accessToken) {
        window.localStorage.setItem('access_token', accessToken);
        return true; //access token or refresh token is success -> if no access token then fail
    }
    return false;
}

const AuthLogic = {
    tryRefreshTokens: async () => {

        const refresh_token = window.localStorage.getItem('refresh_token');

        if (!refresh_token) {
            console.log('missing refresh token, log in again');
            return;
        }

        const res = await fetch(`discovery/refresh_token`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                refresh_token,
            })
        });

        if (res.status == 200) {

            const parsedRes = await res.json();

            const tokenSaveResult = saveTokens(parsedRes.access_token, parsedRes.refresh_token)

            return {
                successResult: tokenSaveResult,
                credentials: {
                    accessToken: window.localStorage.getItem('access_token'),
                    refreshToken: window.localStorage.getItem('refresh_token')
                }
            };
        }

        return { error: 'unauthorized' }
        //refresh token might not come with request
        //has access_token, also it may contain refresh_token
    },

    // runs from ./callback?token=..
    saveCreds: (accessToken, refreshToken) => {

        if (!accessToken) {
            console.log('return error authenticating, redirect user to auth again');
            return false; //not success redirect
        }

        window.localStorage.setItem('access_token', accessToken);

        if (refreshToken) {
            window.localStorage.setItem('refresh_token', refreshToken);
        }

        return true;
    },

    requestAccountId: async (accessToken) => {

        const res = await fetch('https://api.spotify.com/v1/me', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`
            }
        });

        if (res.status == 200) {

            const userData = await res.json();
            //Account.activeUser = userData; // sets current user
            return userData;
        }

        return { errorMessage: 'err getting user id' }
    },

    getAccountInformation: async (accessToken, userId) => {
        const res = await fetch(`https://api.spotify.com/v1/users/${userId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`
            }
        });

        if (res.status == 200) {
            return res
        }
    }
}

export default AuthLogic;