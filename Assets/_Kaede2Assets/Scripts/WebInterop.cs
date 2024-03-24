#if UNITY_WEBGL && !UNITY_EDITOR

using System.Diagnostics;
using System.Runtime.InteropServices;
using Kaede2.Scenario;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI.Web;
using UnityEngine;
// using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Kaede2
{
    public class WebInterop : Singleton<WebInterop>
    {
        private const string DllName = "__Internal";

        [DllImport(DllName)]
        private static extern void RegisterWebInteropGameObject(string gameObjectName);

        [DllImport(DllName)]
        private static extern void RegisterInterops();

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
        public static extern void OnToggleDramaMode(int on);

        public static PlayerScenarioModule Module { get; set; }

        private void Start()
        {
            RegisterWebInteropGameObject(gameObject.name);
            DontDestroyOnLoad(gameObject);

            RegisterInterops();
            OnScenarioListLoaded(JsonUtility.ToJson(MasterScenarioInfo.Instance));
        }

        [Conditional("DEVELOPMENT_BUILD")]
        private static void InteropOptionLog(string message)
        {
            Debug.Log($"[WebInterop] {message}");
        }

        public void ResetPlayer(string unifiedName)
        {
            InteropOptionLog($"resetting player to {unifiedName}");
            var split = unifiedName.Split(':');
            string scenarioName = split[0];
            string languageCode = split[1];

            if (scenarioName == "") return;
            WebBackground.UpdateStatus(WebBackground.Status.ReadyToPlay);
            PlayerScenarioModule.GlobalScenarioName = scenarioName;
            // LocalizationSettings.Instance.SetSelectedLocale(LocalizationSettings.AvailableLocales.Locales
            //     .Find(l => l.Identifier.CultureInfo.TwoLetterISOLanguageName == languageCode));
            // GameManager.ResetPlay();
            SceneManager.LoadScene("ScenarioScene", LoadSceneMode.Single);
        }

        public void SetMasterVolume(float volume)
        {
            InteropOptionLog($"setting master volume to {volume}");
            Module.AudioMasterVolume = Mathf.Clamp01(volume);
        }

        public void SetBGMVolume(float volume)
        {
            InteropOptionLog($"setting BGM volume to {volume}");
            Module.AudioBGMVolume = Mathf.Clamp01(volume);
        }

        public void SetVoiceVolume(float volume)
        {
            InteropOptionLog($"setting voice volume to {volume}");
            Module.AudioVoiceVolume = Mathf.Clamp01(volume);
        }

        public void SetSEVolume(float volume)
        {
            InteropOptionLog($"setting SE volume to {volume}");
            Module.AudioSEVolume = Mathf.Clamp01(volume);
        }

        private static bool fullscreen = false;
        public static bool Fullscreen => fullscreen;

        public void ChangeFullscreen(int status)
        {
            InteropOptionLog($"changing fullscreen to {status > 0}");
            fullscreen = status > 0;
        }

        public void HideMenu()
        {
            InteropOptionLog("hiding menu");
            // TODO
        }

        public void ToggleAutoMode(int on)
        {
            InteropOptionLog($"toggling auto mode to {on > 0}");
            // TODO
        }

        public void ToggleDramaMode(int on)
        {
            InteropOptionLog($"toggling drama mode to {on > 0}");
            // TODO
        }

        public void ToggleWebInput(int on)
        {
            InteropOptionLog($"toggling web input to {on > 0}");
            WebGLInput.captureAllKeyboardInput = on > 0;
        }
    }
}

#endif