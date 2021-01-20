mergeInto(LibraryManager.library, {

  _Connect: function(url) {
    this.webSocket = new WebSocket(Pointer_stringify(url));
  },

  _Close: function() {
    window.alert(this.webSocket);
    this.webSocket.close();
  },

  _SendData: function(arr, size) {
    window.alert(arr);
    for (var i = 0; i < size; i++) {
      window.alert(HEAPU8[arr + i]);
    }
  },

  _SendAlert: function(msg) {
    window.alert(Pointer_stringify(msg));
  },

});
