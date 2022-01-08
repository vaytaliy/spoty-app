import 'bootstrap/dist/css/bootstrap.css';
import React from 'react';
import ReactDOM from 'react-dom';
// import { Routes } from 'react-router';
 import { BrowserRouter, Routes } from 'react-router-dom';
import App from './App';
import Hosting from './components/logic/Hosting';
//import registerServiceWorker from './registerServiceWorker';

const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href');
const rootElement = document.getElementById('root');


ReactDOM.render(
    <BrowserRouter>
      <App />
    </BrowserRouter>
  ,rootElement);

//registerServiceWorker();

