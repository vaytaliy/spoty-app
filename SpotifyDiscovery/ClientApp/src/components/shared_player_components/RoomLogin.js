import React, {useState} from 'react'


const RoomLogin = (props) => {
    const [joinPassword, setJoinPassword] = useState("");

    const handleJoinPasswordSubmit = async (e) => {
        e.preventDefault();

        await props.methods.connect(
            joinPassword); //TODO: invoke parent method
    };

    const handleJoinPassword = (e) => {
        if (joinPassword.length < 8) {
            setJoinPassword(e.target.value);
        }
    };

    return (<form onSubmit={(e) => handleJoinPasswordSubmit(e)}>
        <label>
            Accessing this page requires password
            <input type="text" name="password" value={joinPassword} onChange={(e) => handleJoinPassword(e)} />
        </label>
        <input type="submit" value="Change Password" />
    </form>
    )
};

export default RoomLogin