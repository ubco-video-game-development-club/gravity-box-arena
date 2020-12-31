const myTest = {
  HelloWorld: function() {
    window.alert("Wassup, world?");
  },

  Connect: function() {
    var socket = new WebSocket('ws://localhost:3001');
    socket.onopen = function() {
      socket.send('Hello there, server!');
    };
    socket.onmessage = function(event) {
      console.log(event.data);
      window.alert(event.data);
    };
  }
};

mergeInto(LibraryManager.library, myTest);
