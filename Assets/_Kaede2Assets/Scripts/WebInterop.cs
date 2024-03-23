#if UNITY_WEBGL && !UNITY_EDITOR
#define IS_WEBGL
#else
#define IS_NOT_WEBGL
#endif

using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Kaede2.Scenario;
using Kaede2.Scenario.Framework.Utils;
using UnityEngine;
// using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Kaede2
{
    public class WebInterop : Singleton<WebInterop>
    {
        private const string DLLName = "__Internal";
        private const string ConditionIsWeb = "IS_WEBGL";

        [DllImport(DLLName)] [Conditional(ConditionIsWeb)]
        private static extern void RegisterWebInteropGameObject(string gameObjectName);

        [DllImport(DLLName)] [Conditional(ConditionIsWeb)]
        private static extern void RegisterInterops();

        [DllImport(DLLName)] [Conditional(ConditionIsWeb)]
        public static extern void OnScriptLoaded(string script);

        [DllImport(DLLName)] [Conditional(ConditionIsWeb)]
        public static extern void OnMessageCommand(string speaker, string voiceId, string message);

        [DllImport(DLLName)] [Conditional(ConditionIsWeb)]
        public static extern void OnScenarioStarted();

        [DllImport(DLLName)] [Conditional(ConditionIsWeb)]
        public static extern void OnScenarioFinished();

        [DllImport(DLLName)] [Conditional(ConditionIsWeb)]
        public static extern void OnExitFullscreen();

        [DllImport(DLLName)] [Conditional(ConditionIsWeb)]
        public static extern void OnToggleAutoMode(int on);

        [DllImport(DLLName)] [Conditional(ConditionIsWeb)]
        public static extern void OnToggleDramaMode(int on);

        public static PlayerScenarioModule Module { get; set; }

        [Conditional(ConditionIsWeb)]
        private void Start()
        {
            RegisterWebInteropGameObject(gameObject.name);
            DontDestroyOnLoad(gameObject);

            RegisterInterops();
        }

        [Conditional("DEVELOPMENT_BUILD")]
        private static void InteropOptionLog(string message)
        {
            Debug.Log($"[WebInterop] {message}");
        }

        [Conditional(ConditionIsWeb)]
        public void ResetPlayer(string unifiedName)
        {
            InteropOptionLog($"resetting player to {unifiedName}");
            var split = unifiedName.Split(':');
            string scenarioName = split[0];
            string languageCode = split[1];

            if (scenarioName == "") return;
            PlayerScenarioModule.GlobalScenarioName = scenarioName;
            // LocalizationSettings.Instance.SetSelectedLocale(LocalizationSettings.AvailableLocales.Locales
            //     .Find(l => l.Identifier.CultureInfo.TwoLetterISOLanguageName == languageCode));
            // GameManager.ResetPlay();
            SceneManager.LoadScene("ScenarioScene", LoadSceneMode.Single);
        }

        [Conditional(ConditionIsWeb)]
        public void SetMasterVolume(float volume)
        {
            InteropOptionLog($"setting master volume to {volume}");
            Module.AudioMasterVolume = Mathf.Clamp01(volume);
        }

        [Conditional(ConditionIsWeb)]
        public void SetBGMVolume(float volume)
        {
            InteropOptionLog($"setting BGM volume to {volume}");
            Module.AudioBGMVolume = Mathf.Clamp01(volume);
        }

        [Conditional(ConditionIsWeb)]
        public void SetVoiceVolume(float volume)
        {
            InteropOptionLog($"setting voice volume to {volume}");
            Module.AudioVoiceVolume = Mathf.Clamp01(volume);
        }

        [Conditional(ConditionIsWeb)]
        public void SetSEVolume(float volume)
        {
            InteropOptionLog($"setting SE volume to {volume}");
            Module.AudioSEVolume = Mathf.Clamp01(volume);
        }

        private static bool fullscreen = false;
        public static bool Fullscreen => fullscreen;

        [Conditional(ConditionIsWeb)]
        public void ChangeFullscreen(int status)
        {
            InteropOptionLog($"changing fullscreen to {status > 0}");
            fullscreen = status > 0;
        }

        [Conditional(ConditionIsWeb)]
        public void HideMenu()
        {
            InteropOptionLog("hiding menu");
            // TODO
        }

        [Conditional(ConditionIsWeb)]
        public void ToggleAutoMode(int on)
        {
            InteropOptionLog($"toggling auto mode to {on > 0}");
            // TODO
        }

        [Conditional(ConditionIsWeb)]
        public void ToggleDramaMode(int on)
        {
            InteropOptionLog($"toggling drama mode to {on > 0}");
            // TODO
        }

        [Conditional(ConditionIsWeb)]
        public void ToggleWebInput(int on)
        {
            InteropOptionLog($"toggling web input to {on > 0}");
            WebGLInput.captureAllKeyboardInput = on > 0;
        }
    }
}