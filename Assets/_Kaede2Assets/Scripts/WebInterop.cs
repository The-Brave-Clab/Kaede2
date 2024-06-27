#if UNITY_WEBGL && !UNITY_EDITOR

using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Kaede2.Localization;
using Kaede2.Scenario;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.Web;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kaede2
{
    public class WebInterop : SingletonMonoBehaviour<WebInterop>
    {
        private const string DllName = "__Internal";

        [DllImport(DllName)]
        private static extern void RegisterWebInteropGameObject(string gameObjectName);

        [DllImport(DllName)]
        public static extern void RegisterInterops();

        [DllImport(DllName)]
        public static extern void OnScenarioListLoaded(string scenarioListJson);

        [DllImport(DllName)]
        public static extern void OnScriptLoaded(string script);

        [DllImport(DllName)]
        public static extern void OnScenarioChanged(string scenarioName);

        [DllImport(DllName)]
        public static extern void OnMessageCommand(string speaker, string voiceId, string message);

        [DllImport(DllName)]
        public static extern void OnScenarioStarted();

        [DllImport(DllName)]
        public static extern void OnScenarioFinished();

        [DllImport(DllName)]
        public static extern void OnExitFullscreen();

        [DllImport(DllName)]
        public static extern void OnToggleAutoMode(int on);

        [DllImport(DllName)]
        public static extern void OnToggleContinuousMode(int on);

        [DllImport(DllName)]
        public static extern void OnToggleHideUI(int on);

        public static PlayerScenarioModule Module { get; set; }

        private void Start()
        {
            RegisterWebInteropGameObject(gameObject.name);
            DontDestroyOnLoad(gameObject);

            OnScenarioListLoaded(JsonUtility.ToJson(MasterScenarioInfo.Instance));
        }

        public void ResetPlayer(string unifiedName)
        {
            this.Log($"resetting player to {unifiedName}");
            var split = unifiedName.Split(':');
            string scenarioName = split[0];
            string languageCode = split[1];

            if (scenarioName == "") return;
            WebBackground.UpdateStatus(WebBackground.Status.ReadyToPlay);
            CultureInfo locale = LocalizationManager.AllLocales
                .FirstOrDefault(l => l.TwoLetterISOLanguageName == languageCode) ?? LocalizationManager.AllLocales[0];
            CoroutineProxy.Start(PlayerScenarioModule.Play(scenarioName, locale, LoadSceneMode.Single, null, null));
        }

        public void SetMasterVolume(float volume)
        {
            this.Log($"setting master volume to {volume}");
            Module.AudioMasterVolume = Mathf.Clamp01(volume);
        }

        public void SetBGMVolume(float volume)
        {
            this.Log($"setting BGM volume to {volume}");
            Module.AudioBGMVolume = Mathf.Clamp01(volume);
        }

        public void SetVoiceVolume(float volume)
        {
            this.Log($"setting voice volume to {volume}");
            Module.AudioVoiceVolume = Mathf.Clamp01(volume);
        }

        public void SetSEVolume(float volume)
        {
            this.Log($"setting SE volume to {volume}");
            Module.AudioSEVolume = Mathf.Clamp01(volume);
        }

        private static bool fullscreen = false;
        public static bool Fullscreen => fullscreen;

        public void ChangeFullscreen(int status)
        {
            this.Log($"changing fullscreen to {status > 0}");
            fullscreen = status > 0;
        }

        public void ToggleAutoMode(int on)
        {
            this.Log($"toggling auto mode to {on > 0}");
            Module.AutoMode = on > 0;
        }

        public void ToggleContinuousMode(int on)
        {
            this.Log($"toggling continuous mode to {on > 0}");
            Module.ContinuousMode = on > 0;
        }

        public void ToggleHideUI(int on)
        {
            this.Log($"toggling UI show to {on > 0}");
            Module.PlayerUIController.UIHidden = on > 0;
        }

        public void ToggleWebInput(int on)
        {
            this.Log($"toggling web input to {on > 0}");
            WebGLInput.captureAllKeyboardInput = on > 0;
        }
    }
}

#endif