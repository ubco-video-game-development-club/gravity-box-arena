import React, { useState, useEffect } from 'react';
import './App.css';
import { StringDecoder } from 'string_decoder';
const stringDecoder = new StringDecoder('utf8');

function App() {
  const [clientMessage, setClientMessage] = useState();
  const [serverMessage, setServerMessage] = useState();

  const test = () => {
    var socket = new WebSocket('ws://localhost:3001');
    socket.onopen = () => {
      let packet = createPacket(1);
      socket.send(packet);
      setClientMessage(packet);
    };
    socket.onmessage = (event) => {
      event.data.arrayBuffer().then((buffer) => {
        let data = readPacket(buffer);
        setServerMessage(data);
      });
    };
  };

  const createPacket = (id, key = '', data = []) => {
    let buffer = Buffer.alloc(data.length + 6); //id and key will be 6 bytes in total
    buffer.writeUInt8(id, 0);
    buffer.write(key, 1);
    for (let i = 0; i < data.length; i++) {
      buffer.writeUInt8(data[i], i + 6);
    }
    return buffer;
  };

  const readPacket = (buffer) => {
    let data = new Uint8Array(buffer);
    let packetType = data[0];
    let key = stringDecoder.write(data.slice(1, 6));
    return key;
  };

  useEffect(test, []);

  return (
    <div className='App'>
      <h1>Client Test</h1>
      <p>What if YOU said: {clientMessage}</p>
      <p>But the SERVER said: {serverMessage}</p>
    </div>
  );
}

export default App;
