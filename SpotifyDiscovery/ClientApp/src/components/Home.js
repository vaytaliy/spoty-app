import SpotiPlayer from './SpotiPlayer';
import React, { useState, useEffect } from 'react';
import SongTracker from './SongTracker';

const Home = (props) => {
    
    return (
        <div>
            <SongTracker data={props.data} />
        </div>
    );
}

export default Home 