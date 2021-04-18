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
      navigator.clipboard.readText().then(function(text) {
        unityInstance.SendMessage(gameObjectName, functionName, text);
      }, function() {
        unityInstance.SendMessage(gameObjectName, functionName, "no text in clipboard");
      })
  }
});


