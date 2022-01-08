import React from 'react';

const ConnectedUsers = (props) => {
    
    let connectedPeopleList = [];

    props.data.connectedPeople.forEach((connectedPerson, index) => {
        connectedPeopleList.push(
            <React.Fragment key={index}>
                <div>
                    <p>{connectedPerson["display_name"]}</p>
                    {connectedPerson["images"] && connectedPerson["images"][0] ? <p>{connectedPerson["images"][0]}</p> : null}
                    {connectedPerson.id == props.data.id ? <p>Host</p> : null}
                </div>
            </React.Fragment>)
    });

    return (
        <div>
            {connectedPeopleList}
        </div>
    )
}

export default ConnectedUsers;