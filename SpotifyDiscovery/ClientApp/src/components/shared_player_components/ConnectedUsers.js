import React from 'react';

const ConnectedUsers = (props) => {

    let connectedPeopleList = [];

    props.data.connectedPeople.forEach((connectedPerson, index) => {
        console.log(connectedPerson.id)
        connectedPeopleList.push(
            
            <React.Fragment key={connectedPerson.id}>
                <p>{connectedPerson["display_name"]}</p>
                {connectedPerson["images"] && connectedPerson["images"][0] ? <p>{connectedPerson["images"][0]}</p> : null}
                {connectedPerson.id == props.data.id ? <p>Host</p> : null}
            </React.Fragment>)
    });

    return (
        <div>
            {connectedPeopleList}
        </div>
    )
}

export default ConnectedUsers;