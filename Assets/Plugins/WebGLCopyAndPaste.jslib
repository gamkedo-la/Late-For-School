mergeInto(LibraryManager.library, {
  WriteTextToClipboard: function (text) {
    var dummy = document.createElement("textarea");
    document.body.appendChild(dummy);
    dummy.value = Pointer_stringify(text);
    dummy.select();
    document.execCommand("copy");
    document.body.removeChild(dummy);
  },

  ReadTextFromClipboard: function (gameObjectName, functionName) {
      gameObjectName = UTF8ToString(gameObjectName);
      functionName = UTF8ToString(functionName);
      var text = prompt("Please paste level key here:", "");
      if (text === null) {
        return;
      }
      SendMessage(gameObjectName, functionName, text);
  }
});


