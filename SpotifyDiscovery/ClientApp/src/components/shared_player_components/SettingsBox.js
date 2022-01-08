import React, { useState, useEffect } from 'react'

const SettingsBox = (props) => {
    const [roomPassword, setRoomPassword] = useState("");
    const [roomIsFriendsOnly, setRoomIsFriendsOnly] = useState(false);
    const [roomIsPrivatePage, setRoomIsPrivatePage] = useState(false);


    useEffect(() => {
        if (props.data.roomSettings.password) setRoomPassword(props.data.roomSettings.password);
        if (props.data.roomSettings.isFriendsOnly) setRoomIsFriendsOnly(props.data.roomSettings.isFriendsOnly);
        if (props.data.roomSettings.isPrivate) setRoomIsPrivatePage(props.data.roomSettings.isPrivate);
    }, [props.data.roomSettings])

    const handleNewPasswordSubmit = async (e) => {
        e.preventDefault();

        if (roomPassword.length <= 8) {
            await props.methods.changeRoomProperties({
                changeType: "password_change",
                setPassword: roomPassword
            });
        }
    };


    const handleIsPublicRoomCheckbox = async (e) => {

        const newVal = !roomIsPrivatePage;
        await props.methods.changeRoomProperties({
            changeType: "room_public_access",
            setIsPrivateRoom: newVal
        })
        setRoomIsPrivatePage(newVal)
    };

    const handleIsFriendsOnlyCheckbox = async (e) => {

        const newVal = !roomIsFriendsOnly;
        console.log(roomIsFriendsOnly)
        await props.methods.changeRoomProperties({
            changeType: "room_friends_only",
            setIsFriendsOnly: newVal
        })
        setRoomIsFriendsOnly(newVal)

        if (newVal === true) {
            setRoomIsPrivatePage(true)
        }
    };

    const handleRoomPassword = (e) => {
        if (roomPassword.length < 8) {
            setRoomPassword(e.target.value)
        }
    };

    return (
        <div>
            <form onSubmit={(e) => handleNewPasswordSubmit(e)}>
                <label>
                    Set password (keep empty for no password)
                    <input type="text" name="password" value={roomPassword} onChange={(e) => handleRoomPassword(e)} />
                </label>
                <input type="submit" value="Change Password" />
            </form>
            <label>
                Room friends only
                <input type="checkbox" id="roomIsFriendsOnly" checked={roomIsFriendsOnly} onChange={(e) => handleIsFriendsOnlyCheckbox(e)} name="roomIsFriendsOnly" />
            </label>
            <label>
                Room is available on public page
                <input type="checkbox" id="setRoomIsOnPublicPage" checked={roomIsPrivatePage} onChange={(e) => handleIsPublicRoomCheckbox(e)} name="setRoomIsOnPublicPage" />
            </label>
        </div>
    )

};

export default SettingsBox;