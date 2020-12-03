import React, { useState, useEffect } from 'react';
import './App.css';

function App() {
  const [status, setStatus] = useState();

  const test = () => {
    var socket = new WebSocket('ws://localhost:3001');
    socket.onopen = function() {
      socket.send('Hello there!');
    };
    socket.onmessage = function(event) {
      console.log(event.data);
      setStatus(event.data);
    };
  };

  useEffect(test, []);

  return (
    <div className='App'>
      <h1>Client Test</h1>
      <p>The server said: {status}</p>
    </div>
  );
}

export default App;
