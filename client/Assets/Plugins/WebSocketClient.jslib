mergeInto(LibraryManager.library, {

  _SendAlert: function(msg) {
    window.alert(Pointer_stringify(msg));
  },

  _Connect: function(url) {
    console.log("javascript connect");
    this.webSocket = new WebSocket(Pointer_stringify(url));
  },

  _ConnectTwo: function(url) {
    console.log("javascript connect two");
    window.alert(Pointer_stringify(url));
    this.webSocket = new WebSocket(Pointer_stringify(url));
  },

  _PeePeePooPoo: function(msg) {
    window.alert(Pointer_stringify(msg));
  },

  _Close: function() {
    window.alert(this.webSocket);
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
