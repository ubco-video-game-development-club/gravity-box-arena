mergeInto(LibraryManager.library, {

  _SendAlert: function(msg) {
    window.alert(Pointer_stringify(msg));
  },

  _Connect: function(url) {
    console.log("javascript connect");
    this.webSocket = new WebSocket(Pointer_stringify(url));
  },

  _Close: function() {
    console.log(this.webSocket);
    this.webSocket.close();
  },

  _SendData: function(arr, size) {
    var data = HEAPU8[arr];
    window.alert(data);
    // for (var i = 0; i < size; i++) {
    //   window.alert(data);
    //   this.webSocket.send(data);
    // }
  },

});
