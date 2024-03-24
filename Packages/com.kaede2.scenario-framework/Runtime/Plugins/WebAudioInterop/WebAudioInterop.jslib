var AudioPlugin = {
    $AudioPluginAnalyzers: {},
    $AudioPluginFunctions: {

        FindAudioChannel: function (samples) {
            var channel = null;

            if (typeof WEBAudio === 'undefined') return false;
            var keys = Object.keys(WEBAudio.audioInstances);
            if (keys.length > 1) {
                for (var i = keys.length - 1; i >= 0; i--) {
                    var key = keys[i];
                    var instance = WEBAudio.audioInstances[key];
                    if (instance) {
                        if (typeof instance.source === "undefined") {
                            // this is a buffer instance
                            continue;
                        }
                        var pSource = WEBAudio.audioInstances[key].source;
                        if (pSource != null && pSource.buffer != null && pSource.buffer.length === samples) {
                            channel = WEBAudio.audioInstances[key];
                            break;
                        }
                    }
                }
            }

            if (channel == null) {
                console.warn("Did not find an audio channel that matches!");
            }

            return channel;
        },

        FixChannelResumeInfinite: function (channel) {
            if (channel == null) {
                return;
            }

            if (channel.source.isPausedMockNode) {
                if (typeof channel.source.playbackPausedAtPosition !== "undefined") {
                    if (!isFinite(channel.source.playbackPausedAtPosition)) {
                        channel.source.playbackPausedAtPosition = 0.0;
                    }
                }
            }
        }

    },

    StartAudioSampling: function (namePtr, samples, bufferSize) {

        var name = UTF8ToString(namePtr);
        if (AudioPluginAnalyzers[name] != null) return;

        var analyzer = null;
        var channel = null;

        try {
            channel = AudioPluginFunctions.FindAudioChannel(samples);

            if (channel == null) {
                return false;
            }

            AudioPluginFunctions.FixChannelResumeInfinite(channel);

            if (channel.source.isPausedMockNode) {
                channel.resume();
                if (channel.source.isPausedMockNode) {
                    console.log("Audio source " + name + " is a mock node");
                    return false;
                }
            }

            analyzer = WEBAudio.audioContext.createAnalyser();
            analyzer.fftSize = bufferSize * 2;
            analyzer.smoothingTimeConstant = 0;
            channel.source.connect(analyzer);

            AudioPluginAnalyzers[name] = {
                analyzer: analyzer,
                channel: channel
            };

            return true;
        } catch (e) {
            console.error("Failed to connect analyser to source " + e);

            if (analyzer != null && channel.source != null) {
                if (typeof channel.source.isPausedMockNode === "undefined" || !channel.source.isPausedMockNode) {
                    channel.source.disconnect(analyzer);
                }
            }
        }

        return false;
    },

    CloseAudioSampling: function (namePtr) {
        var name = UTF8ToString(namePtr);
        var analyzerObj = AudioPluginAnalyzers[name];

        if (analyzerObj != null) {
            var success = false;
            try {
                if (typeof analyzerObj.channel.source !== "undefined" &&
                    (typeof analyzerObj.channel.source.isPausedMockNode === "undefined" || !analyzerObj.channel.source.isPausedMockNode))
                    analyzerObj.channel.source.disconnect(analyzerObj.analyzer);
                success = true;
            } catch (e) {
                console.warn("Failed to disconnect analyser " + name + " from source " + e);
            } finally {
                delete AudioPluginAnalyzers[name];
                return true;
            }
        }

        return false;
    },

    GetAudioSamples: function (namePtr, bufferPtr, bufferSize) {
        var name = UTF8ToString(namePtr);
        if (AudioPluginAnalyzers[name] == null) {
            console.warn("analyzer with name " + name + " not found!");
            return false;
        }
        try {
            var buffer = new Float32Array(bufferSize);

            var analyzerObj = AudioPluginAnalyzers[name];

            if (analyzerObj == null) {
                console.warn("Could not find analyzer " + name + " to get lipsync data for");
                return false;
            }

            analyzerObj.analyzer.getFloatFrequencyData(buffer);
            for (var i = 0; i < buffer.length; i++) {
                HEAPF32[(bufferPtr >> 2) + i] = Math.pow(10, buffer[i] / 20) * 4;
            }
            return true;
        } catch (e) {
            console.error("Failed to get lipsync sample data " + e);
        }

        return false;
    }
};

autoAddDeps(AudioPlugin, '$AudioPluginAnalyzers');
autoAddDeps(AudioPlugin, '$AudioPluginFunctions');
mergeInto(LibraryManager.library, AudioPlugin);