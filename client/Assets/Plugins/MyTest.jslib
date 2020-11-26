const myTest = {
  HelloWorld: function() {
    window.alert("Wassup, world?");
  }
};

mergeInto(LibraryManager.library, myTest);
