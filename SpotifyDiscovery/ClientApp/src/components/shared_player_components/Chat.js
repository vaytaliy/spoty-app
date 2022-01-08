import React, {useState} from 'react';
import Hosting from './../logic/Hosting'

const Chat = (props) => {
    const [messageBoxContents, setMessageBoxContents] = useState("");
    
    let chatMessages = [];

    props.data.chatContents.map((chatMessage, index) => {
        chatMessages.push(
            <React.Fragment key={index}>
                <div>
                    <p className={`chat-active-user-${chatMessage.thisUser}`}>{chatMessage.thisUser ? "You" : chatMessage.sender}: {chatMessage.text}</p>
                </div>
            </React.Fragment>)
    });



    const handleMessageSubmit = async (e) => {
        e.preventDefault();

        if (messageBoxContents.length > 0) {
            const messageInfo = {
                text: messageBoxContents
            }
            setMessageBoxContents("");

            await Hosting.sendMessageInRoom(messageInfo); //TODO Invoke parent method!
        }
    };

    const handleMessageBoxChange = (e) => {
        if (messageBoxContents.length <= 100) { setMessageBoxContents(e.target.value); }
    };

    return (
        <div>
            <br />
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
    )
}

export default Chat;