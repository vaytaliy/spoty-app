import React, {useEffect, useState} from 'react'

const RoomError = (props) => { //set room error from parent component

    if (props.data.roomError && props.data.roomError.errorType) {
        return (
            <div>
                <h2>{props.data.roomError.errorType}</h2> 
                <div>{props.data.roomError.errorDescription}</div>
                {props.data.roomError.errorType === "friendlist_error" ? <a href="/">Take me to main page :(</a> : null}
            </div>
        )
    }
    return null;
}

export default RoomError