mergeInto(LibraryManager.library, {

  ConnectInternal: function (url) {
    this.webSocket = new WebSocket(url);
  },

  SendAlertInternal: function (msg) {
    window.alert(Pointer_stringify(msg));
  },

});
