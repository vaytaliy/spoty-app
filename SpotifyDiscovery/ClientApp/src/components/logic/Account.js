import AppInfo from '../../constants';

const Account = {

    activeUser: null,

    requestCurrentSettings: async () => { //TODO
        const resp = await fetch(
            `room/sharing_settings`
        );
    }
}

export default Account;