var WebInterop = {

    $InteropGameObject: null,

    $InteropCallbacks: {
        OnScenarioListLoaded: function(scenarioList) { },
        OnScriptLoaded: function(script) { },
        OnScenarioChanged: function(scenarioName) { },
        OnMessageCommand: function(speaker, voiceId, message) { },
        OnScenarioStarted: function() { },
        OnScenarioFinished: function() { },
        OnExitFullscreen: function() { },
        OnToggleAutoMode: function(on) { },
        OnToggleDramaMode: function(on) { }
    },

    $InteropFunctions: {
        ResetPlayer: function (scriptName, languageCode) {
            let unifiedName = [scriptName, languageCode].join(":");
            SendMessage(InteropGameObject, "ResetPlayer", unifiedName);
        },

        SetVolume: function (master, bgm, voice, sfx) {
            SendMessage(InteropGameObject, "SetMasterVolume", master);
            SendMessage(InteropGameObject, "SetBGMVolume", bgm);
            SendMessage(InteropGameObject, "SetVoiceVolume", voice);
            SendMessage(InteropGameObject, "SetSEVolume", sfx);
        },

        EnterFullscreen: function() {
            SendMessage(InteropGameObject, "ChangeFullscreen", 1);
        },

        ExitFullscreen: function() {
            SendMessage(InteropGameObject, "ChangeFullscreen", 0);
            SendMessage(InteropGameObject, "HideMenu");
        },

        ToggleAutoMode: function(on) {
            if (on)
                SendMessage(InteropGameObject, "ToggleAutoMode", 1);
            else
                SendMessage(InteropGameObject, "ToggleAutoMode", 0);
        },

        ToggleDramaMode: function(on) {
            if (on)
                SendMessage(InteropGameObject, "ToggleDramaMode", 1);
            else
                SendMessage(InteropGameObject, "ToggleDramaMode", 0);
        },

        ToggleInputInterception: function(on) {
            if (on)
                SendMessage(InteropGameObject, 'ToggleWebInput', 1);
            else
                SendMessage(InteropGameObject, 'ToggleWebInput', 0);
        },
    },

    // GetWebGLScriptName: function () {
    //     var scriptName;
    //     if (typeof _y3advScriptName === 'undefined') {
    //         console.error("Please set the value of _y3advScriptName before initializing the Unity Player!");
    //         scriptName = "";
    //     }
        
    //     scriptName = _y3advScriptName;

    //     var bufferSize = lengthBytesUTF8(scriptName) + 1;
    //     var buffer = _malloc(bufferSize);
    //     stringToUTF8(scriptName, buffer, bufferSize);

    //     return buffer;
    // },

    RegisterWebInteropGameObject: function (gameObjectName) {
        InteropGameObject = UTF8ToString(gameObjectName);
        console.log("InteropGameObject registered as " + InteropGameObject);
    },

    RegisterInterops: function () {
        console.log("Registering interops...");
        b352a51964f6fc4813af8f08d403ec0d(
            InteropCallbacks,
            InteropFunctions.ResetPlayer,
            InteropFunctions.SetVolume,
            InteropFunctions.EnterFullscreen,
            InteropFunctions.ExitFullscreen,
            InteropFunctions.ToggleAutoMode,
            InteropFunctions.ToggleDramaMode,
            InteropFunctions.ToggleInputInterception);
    },

    OnScenarioListLoaded: function (scenarioListJson) {
        InteropCallbacks.OnScenarioListLoaded(JSON.parse(UTF8ToString(scenarioListJson)));
    },

    OnScriptLoaded: function (script) {
        InteropCallbacks.OnScriptLoaded(UTF8ToString(script));
    },

    OnScenarioChanged: function (scenarioName) {
        InteropCallbacks.OnScenarioChanged(UTF8ToString(scenarioName));
    },

    OnMessageCommand: function (speaker, voiceId, message) {
        InteropCallbacks.OnMessageCommand(UTF8ToString(speaker), UTF8ToString(voiceId), UTF8ToString(message));
    },

    OnScenarioStarted: function () {
        InteropCallbacks.OnScenarioStarted();
    },

    OnScenarioFinished: function () {
        InteropCallbacks.OnScenarioFinished();
    },

    OnExitFullscreen: function () {
        InteropCallbacks.OnExitFullscreen();
    },

    OnToggleAutoMode: function (on) {
        InteropCallbacks.OnToggleAutoMode(on > 0);
    },

    OnToggleDramaMode: function (on) {
        InteropCallbacks.OnToggleDramaMode(on > 0);
    }

};

autoAddDeps(WebInterop, '$InteropGameObject');
autoAddDeps(WebInterop, '$InteropCallbacks');
autoAddDeps(WebInterop, '$InteropFunctions');
mergeInto(LibraryManager.library, WebInterop);