import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import Hosting from './logic/Hosting';
import SpotiPlayer from './SpotiPlayer';
import Auth from './logic/Auth';


const SharedPlayer = (props) => {

    let { id } = useParams();

    const [connectedPeople, setConnectedPeople] = useState(new Map());
    const [messageBoxContents, setMessageBoxContents] = useState("");
    const [chatContents, setChatContents] = useState([])
    let connectedPeopleList = [];
    let chatMessages = [];

    connectedPeople.forEach((connectedPerson, index) => {
        connectedPeopleList.push(
            <React.Fragment key={index}>
                <div>
                    <p>{connectedPerson["display_name"]}</p>
                    {connectedPerson["images"] && connectedPerson["images"][0] ? <p>{connectedPerson["images"][0]}</p> : null}
                    {connectedPerson.id == id ? <p>Host</p> : null}
                </div>
            </React.Fragment>)
    });


    chatContents.map((chatMessage, index) => {
        chatMessages.push(
            <React.Fragment key={index}>
                <div>
                    <p className={`chat-active-user-${chatMessage.thisUser}`}>{chatMessage.thisUser ? "You" : chatMessage.sender}: {chatMessage.text}</p>
                </div>
            </React.Fragment>)
    });

    const handleSetConnectedPeople = (connectedPeopleMap) => {
        setConnectedPeople(connectedPeopleMap);
    }


    const handleMessageBoxChange = (e) => {
        if (messageBoxContents.length <= 100) { setMessageBoxContents(e.target.value); }
    }

    const handleReceiveChatMessage = async (incomingMessage, spotifyIdSender) => {
        const account = await Auth.requestAccountId(window.localStorage.getItem('access_token'));
        
        if (account) {
            if (spotifyIdSender === account.id) {
                incomingMessage.thisUser = true
            } else if (spotifyIdSender !== account.id) {
                incomingMessage.thisUser = false
            }

            setChatContents(oldChatContents => [...oldChatContents, incomingMessage]);
        }
    }

    const handleMessageSubmit = async (e) => {
        e.preventDefault();

        if (messageBoxContents.length > 0) {
            const messageInfo = {
                text: messageBoxContents
            }
            setMessageBoxContents("");

            await Hosting.sendMessageInRoom(messageInfo);
        }

    }

    return (
        <div>
            <SpotiPlayer data={props.data} playerControl={props.playerControl} sharing={true} listeningTo={id} uiControls={{ handleSetConnectedPeople, handleReceiveChatMessage }} />
            <br />
            {connectedPeopleList}
            <div>Welcome to shared player</div>
            <div id="chatContainer">
                <div id="messageArea">
                    message area
                    {chatMessages}
                </div>

                <form onSubmit={(e) => handleMessageSubmit(e)}>
                    <label>
                        Send message:
                        <input type="text" name="message" value={messageBoxContents} onChange={(e) => handleMessageBoxChange(e)} />
                    </label>
                    <input type="submit" value="Send message" />
                </form>
            </div>
        </div>
    );
}

export default SharedPlayer