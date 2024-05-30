using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Kaede2.Input;
using Kaede2.Localization;
using Kaede2.Scenario.Framework;
using Kaede2.Scenario.Framework.Commands;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
#if UNITY_WEBGL && !UNITY_EDITOR
using Kaede2.Web;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Sprite = UnityEngine.Sprite;

namespace Kaede2.Scenario
{
    public class PlayerScenarioModule : ScenarioModule
    {
        private static bool? lastAutoMode = null;
        private static bool? lastContinuousMode = null;

        private List<string> statements;
        private List<Command> commands;
        private int currentCommandIndex;

        private List<ResourceLoader.HandleBase> resourceHandles;

        private CultureInfo backupLocale;

        [SerializeField]
        private PlayerUIController uiController;

        [SerializeField]
        private AudioManager audioManager;

#if UNITY_EDITOR
        [Header("For editor only")]
        public SerializableCultureInfo defaultLanguage;
        public string defaultScenarioName;
#endif

        public override string ScenarioName => scenarioName;

        public override IReadOnlyList<string> Statements => statements.AsReadOnly();
        public override IReadOnlyList<Command> Commands => commands.AsReadOnly();

        public override bool AutoMode
        {
            get => lastAutoMode ?? base.AutoMode;
            set
            {
                lastAutoMode = base.AutoMode = value;
#if UNITY_WEBGL && !UNITY_EDITOR
                WebInterop.OnToggleAutoMode(value ? 1 : 0);
#endif
            }
        }

        public override bool ContinuousMode
        {
            get => lastContinuousMode ?? base.ContinuousMode;
            set
            {
                lastContinuousMode = base.ContinuousMode = value;
#if UNITY_WEBGL && !UNITY_EDITOR
                WebInterop.OnToggleContinuousMode(value ? 1 : 0);
#endif
            }
        }

        public override bool Fixed16By9
        {
            get => GameSettings.Fixed16By9;
            set => GameSettings.Fixed16By9 = value;
        }
        public override float AudioMasterVolume
        {
            get => GameSettings.AudioMasterVolume;
            set => GameSettings.AudioMasterVolume = value;
        }
        public override float AudioBGMVolume
        {
            get => GameSettings.AudioBGMVolume;
            set => GameSettings.AudioBGMVolume = value;
        }
        public override float AudioSEVolume
        {
            get => GameSettings.AudioSEVolume;
            set => GameSettings.AudioSEVolume = value;
        }
        public override float AudioVoiceVolume
        {
            get => GameSettings.AudioVoiceVolume;
            set => GameSettings.AudioVoiceVolume = value;
        }

        public override int CurrentCommandIndex
        {
            get => currentCommandIndex;
            protected set => currentCommandIndex = value;
        }

        public override bool MesClicked => InputManager.InputAction.Scenario.Next.triggered || uiController.MesButton.Pressed;

        public override UIController UIController => uiController;
        public override AudioManager AudioManager => audioManager;

        public PlayerUIController PlayerUIController => uiController;


        private static string scenarioName;
        private static CultureInfo scenarioLanguage;
        private static ScenarioState scenarioStateToBeRestored;
        private static Action scenarioEndCallback;

        public static string CurrentScenario => scenarioName;
        public static CultureInfo CurrentLanguage => scenarioLanguage;

        public static IEnumerator Play(string scenario, CultureInfo language, LoadSceneMode loadSceneMode, ScenarioState stateToBeRestored, Action endCallback)
        {
            scenarioName = scenario;
            scenarioLanguage = language;
            scenarioStateToBeRestored = stateToBeRestored;
            scenarioEndCallback = endCallback;

            yield return SceneManager.LoadSceneAsync("ScenarioScene", loadSceneMode);
        }

        public void GoBackToPreviousSceneBeforeEnd()
        {
            GoBackToPreviousScene(true);
        }

        private void GoBackToPreviousSceneBeforeEnd(InputAction.CallbackContext context)
        {
            GoBackToPreviousSceneBeforeEnd();
        }

        #region EventFunctions

        protected override void Awake()
        {
            base.Awake();

            if (ScenarioRunMode.Args.TestMode)
                Time.timeScale = 10.0f;

            statements = new();
            commands = new();
            currentCommandIndex = -1;

            resourceHandles = new();

            backupLocale = null;

#if UNITY_IOS
            UnityEngine.iOS.Device.hideHomeButton = true;
#endif
#if UNITY_WEBGL && !UNITY_EDITOR
            WebInterop.Module = this;
            OnMesCommand += WebInterop.OnMessageCommand;
#endif

            InputManager.InputAction.Scenario.Enable();
            // show log action and go back action will be enabled after init_end
            InputManager.InputAction.Scenario.ShowLog.Disable();
            InputManager.InputAction.Scenario.GoBack.Disable();

            if (ScenarioRunMode.Args.SpecifiedScenario)
            {
                scenarioName = ScenarioRunMode.Args.SpecifiedScenarioName;
            }
        }

        private IEnumerator Start()
        {
            // we might want to do a global initialization here
            // since we might skip the splash screen
            if (GlobalInitializer.CurrentStatus != GlobalInitializer.Status.Done)
                yield return GlobalInitializer.Initialize();

            yield return Resources.UnloadUnusedAssets();
            yield return SceneTransition.Fade(0);

#if UNITY_EDITOR
            // in editor, we might directly run the scenario scene
            // in this case, we set a default scenario name and language
            if (string.IsNullOrEmpty(scenarioName))
            {
                scenarioName = defaultScenarioName;
                this.Log("Using default scenario name in editor");
            }

            if (scenarioLanguage == null)
            {
                scenarioLanguage = defaultLanguage;
                this.Log("Using default scenario language in editor");
            }
#endif

            if (ScenarioRunMode.Args.SpecifiedScenario)
            {
                // if we entered through command line args, we force the scenario language to be the specified one
                // the logic of choosing specific locale is inside GameSettings
                scenarioLanguage = GameSettings.CultureInfo;
            }

            var scriptHandle = ResourceLoader.LoadScenarioScriptText(ScenarioName);
            // we could just release this right after getting the text string instead of releasing with other handles,
            // but it will usually unload the scenario bundle too which we are still going to use right after this,
            // so we will release it with other handles
            resourceHandles.Add(scriptHandle);
            yield return scriptHandle.Send();

            // TODO: error handling
            var scriptText = scriptHandle.Result.text;

            // download translation if needed
            var targetLocale = LocalizationManager.AllLocales.FirstOrDefault(l => l.TwoLetterISOLanguageName == "ja")!;
            string translation = "";

            if (ScenarioRunMode.Args.OverrideTranslation)
            {
                if (File.Exists(ScenarioRunMode.Args.OverrideTranslationFile))
                    translation = File.ReadAllText(ScenarioRunMode.Args.OverrideTranslationFile);
                else
                    this.LogWarning($"Failed to load override translation file: {ScenarioRunMode.Args.OverrideTranslationFile}");
            }
            else if (scenarioLanguage != null && scenarioLanguage.TwoLetterISOLanguageName != "ja")
            {
                yield return LocalizeScript.DownloadTranslation(ScenarioName, scenarioLanguage.TwoLetterISOLanguageName, t => translation = t);

                if (string.IsNullOrEmpty(translation))
                    this.LogWarning($"Failed to download translation for {scenarioLanguage}. Defaulting to {targetLocale}. This should not happen if entered from scenario selection menu.");
            }

            if (!string.IsNullOrEmpty(translation))
            {
                targetLocale = scenarioLanguage;
                scriptText = LocalizeScript.ApplyTranslation(scriptText, translation);
                this.Log($"Applied translation for {scenarioLanguage}");
            }

            // change locale if needed
            if (!Equals(targetLocale, LocalizationManager.CurrentLocale))
            {
                backupLocale = LocalizationManager.CurrentLocale;
                LocalizationManager.CurrentLocale = targetLocale;
                this.Log($"Locale changed to {targetLocale!.EnglishName}");
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            WebInterop.OnScriptLoaded(scriptText);
#endif
            var originalStatements = GetStatementsFromScript(scriptText);

            Dictionary<string, List<string>> includeFiles = new();
            yield return PreloadIncludeFiles(originalStatements, includeFiles);

            var includePreprocessedStatements = PreprocessInclude(originalStatements, includeFiles);
            statements = Function.PreprocessFunctions(includePreprocessedStatements);
            yield return PreprocessAliasesAndVariables(statements);

            yield return PreloadResources();

            commands = statements.Select(ParseStatement).ToList();

            StartCoroutine(Execute());
        }

        protected void OnDestroy()
        {
            foreach (var handle in resourceHandles)
            {
                handle.Dispose();
            }

            InputManager.InputAction?.Scenario.Disable();

#if UNITY_IOS
            UnityEngine.iOS.Device.hideHomeButton = false;
#endif
#if UNITY_WEBGL && !UNITY_EDITOR
            WebInterop.Module = null;
#endif
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        private void OnEnable()
        {
            InputManager.InputAction.Scenario.GoBack.performed += GoBackToPreviousSceneBeforeEnd;
        }

        private void OnDisable()
        {
            if (InputManager.InputAction != null)
            {
                InputManager.InputAction.Scenario.GoBack.performed -= GoBackToPreviousSceneBeforeEnd;
            }
        }
#endif

        #endregion

        #region CommandCallOverrides

        public override IEnumerator InitEnd()
        {
            UIController.LoadingCanvas.gameObject.SetActive(false);
            this.Log("Scenario initialized");
            if (scenarioStateToBeRestored != null)
            {
                this.Log("Restoring sync point");
                RestoreState(scenarioStateToBeRestored);
                scenarioStateToBeRestored = null;
            }


#if UNITY_WEBGL && !UNITY_EDITOR
            while (WebBackground.CurrentStatus == WebBackground.Status.ReadyToPlay)
                yield return null;
            WebInterop.OnScenarioStarted();
#else
            // these actions will not be enabled in web build
            InputManager.InputAction.Scenario.ShowLog.Enable();
            InputManager.InputAction.Scenario.GoBack.Enable();
#endif

            yield break;
        }

        public override IEnumerator End()
        {
            this.Log("Scenario ended");

            if (ContinuousMode)
            {
                var nextScenarioInfo = MasterScenarioInfo.GetNextScenarioInfo(scenarioName);
                if (nextScenarioInfo != null)
                {
                    scenarioName = nextScenarioInfo.ScenarioName;
                    // preserve scenario language
                    scenarioStateToBeRestored = null; // there will be no sync point to restore when enter the next scenario
                    // preserve scenario finished callback

                    CoroutineProxy.Start(Reload());
                    yield break;
                }
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            WebBackground.UpdateStatus(WebBackground.Status.Finished);
            WebInterop.OnScenarioFinished();
            yield break;
#else
            // We entered through command line args
            if (ScenarioRunMode.Args.SpecifiedScenario)
            {
                Application.Quit(0);
            }

            scenarioEndCallback?.Invoke();

            GoBackToPreviousScene(false);
#endif
        }

        private void GoBackToPreviousScene(bool saveState)
        {
            Paused = true;

            if (saveState)
            {
                // TODO: implement save state
                this.LogWarning("Save state not implemented");
                // this.Log("Unfinished state saved");
            }

            if (backupLocale != null)
            {
                LocalizationManager.CurrentLocale = backupLocale;
                this.Log($"Restored locale to {backupLocale.EnglishName}");
            }

            // TODO: implement go back to previous scene
            this.LogWarning("Go back to previous scene not implemented");
            // this.Log("Going back to previous scene");
        }

        private IEnumerator Reload()
        {
            // no fade in/out
#if UNITY_WEBGL && !UNITY_EDITOR
            WebInterop.OnScenarioChanged(scenarioName);
            // on web build the scene is loaded with single mode so just load it again with single mode
            yield return Play(scenarioName, scenarioLanguage, LoadSceneMode.Single, scenarioStateToBeRestored, scenarioEndCallback);
#else
            // on other platforms the scene is loaded additively, so we need to unload the old one
            yield return Play(scenarioName, scenarioLanguage, LoadSceneMode.Additive, scenarioStateToBeRestored, scenarioEndCallback);
            yield return SceneManager.UnloadSceneAsync(gameObject.scene);
#endif
        }

        #endregion

        #region ResourceLoading

        private IEnumerator SendHandleWithFinishCallback<T>(ResourceLoader.LoadAddressableHandle<T> handle, Action<T> callback) where T : UnityEngine.Object
        {
            yield return handle.Send();
            if (handle.Result == null)
            {
                this.LogError($"Failed to load asset {handle.AssetAddress}");
            }

            callback(handle.Result);
        }

        private IEnumerator SendLive2DHandleWithFinish(ResourceLoader.LoadLive2DHandle handle, string resourceName)
        {
            yield return handle.Send();
            if (handle.Result == null)
            {
                this.LogError($"Failed to Live2D model {resourceName}");
            }

            ScenarioResource.Actors[resourceName] = handle.Result;
        }

        public override IEnumerator LoadResource(Resource.Type type, string resourceName)
        {
            switch (type)
            {
                case Resource.Type.Sprite:
                {
                    var handle = ResourceLoader.LoadScenarioSprite(resourceName);
                    resourceHandles.Add(handle);
                    return SendHandleWithFinishCallback(handle, s => ScenarioResource.Sprites[resourceName] = s);
                }
                case Resource.Type.Still:
                {
                    var handle = ResourceLoader.LoadScenarioStill(ScenarioName, resourceName);
                    resourceHandles.Add(handle);
                    return SendHandleWithFinishCallback(handle, t => ScenarioResource.Stills[resourceName] = t);
                }
                case Resource.Type.Background:
                {
                    var handle = ResourceLoader.LoadScenarioBackground(resourceName);
                    resourceHandles.Add(handle);
                    return SendHandleWithFinishCallback(handle, t => ScenarioResource.Backgrounds[resourceName] = t);
                }
                case Resource.Type.SE:
                {
                    var handle = ResourceLoader.LoadScenarioSoundEffect(resourceName);
                    resourceHandles.Add(handle);
                    return SendHandleWithFinishCallback(handle, a => ScenarioResource.SoundEffects[resourceName] = a);
                }
                case Resource.Type.BGM:
                {
                    var handle = ResourceLoader.LoadScenarioBackgroundMusic(resourceName);
                    resourceHandles.Add(handle);
                    return SendHandleWithFinishCallback(handle, a => ScenarioResource.BackgroundMusics[resourceName] = a);
                }
                case Resource.Type.Voice:
                {
                    var handle = ResourceLoader.LoadScenarioVoice(ScenarioName, resourceName);
                    resourceHandles.Add(handle);
                    return SendHandleWithFinishCallback(handle, a => ScenarioResource.Voices[resourceName] = a);
                }
                case Resource.Type.TransformPrefab:
                {
                    CharacterId id = (CharacterId)int.Parse(resourceName);
                    var handle = ResourceLoader.LoadScenarioTransformEffectSprite(id);
                    resourceHandles.Add(handle);
                    return SendHandleWithFinishCallback(handle, s => ScenarioResource.TransformImages[id] = s);
                }
                case Resource.Type.Actor:
                {
                    var handle = ResourceLoader.LoadLive2DModel(resourceName);
                    resourceHandles.Add(handle);
                    return SendLive2DHandleWithFinish(handle, resourceName);
                }
                case Resource.Type.AliasText:
                {
                    
                    var handle = ResourceLoader.LoadScenarioAliasText(ScenarioName, resourceName);
                    resourceHandles.Add(handle);
                    return SendHandleWithFinishCallback(handle, t => ScenarioResource.AliasText = t);
                }
            }

            return null;
        }

        #endregion

        #region ScriptProcessing

        private static List<string> GetStatementsFromScript(string script)
        {
            var lines = script.Split('\n', '\r');
            List<string> result = new List<string>(lines.Length);
            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("-")) continue; // I really don't think there's a line starts with -
                //trimmed = trimmed.Split(new[] {"//"}, StringSplitOptions.None)[0];
                if (trimmed.StartsWith("//")) continue; // we don't treat // in a valid line as comments any more.
                if (trimmed == "") continue;
                result.Add(trimmed);
            }

            return result;
        }
        private IEnumerator PreloadIncludeFiles(List<string> statements, Dictionary<string, List<string>> includeFiles)
        {
            var includeStatements = statements.Where(s => s.StartsWith("include")).ToList();

            List<Tuple<string, ResourceLoader.LoadAddressableHandle<TextAsset>>> includeHandles = new();
            foreach (var s in includeStatements)
            {
                string[] args = s.Split(new[] { '\t' }, StringSplitOptions.None);
                string includeFileName = args[1];
                if (includeFileName == "define_function") includeFileName = "define_functions"; // a fix
                // for now the include files are only in defines
                var includeHandle = ResourceLoader.LoadScenarioDefineText(includeFileName);
                includeHandles.Add(new(includeFileName, includeHandle));
            }

            if (includeHandles.Count == 0)
                yield break;

            CoroutineGroup group = new();
            foreach (var (_, handle) in includeHandles)
                group.Add(handle.Send(), this);
            yield return group.WaitForAll();

            foreach (var (fileName, handle) in includeHandles)
            {
                var includeFileContent = handle.Result.text;
                // include/define files are in a self-contained bundle
                // since we are not going to use them after this, it's ok to release the handles
                handle.Dispose();

                this.Log($"Pre-Loaded include file {fileName}");
                var includeFileStatements = GetStatementsFromScript(includeFileContent);
                includeFiles[fileName] = includeFileStatements;
                yield return PreloadIncludeFiles(includeFileStatements, includeFiles);
            }
        }

        private static List<string> PreprocessInclude(List<string> originalStatements,
            Dictionary<string, List<string>> includeFiles)
        {
            List<string> outputStatements = new();

            foreach (var s in originalStatements)
            {
                if (!s.StartsWith("include"))
                {
                    outputStatements.Add(s);
                    continue;
                }

                string[] args = s.Split(new[] { '\t' }, StringSplitOptions.None);
                string includeFileName = args[1];
                if (includeFileName == "define_function") includeFileName = "define_functions"; // a fix
                var includeStatements = includeFiles[includeFileName];
                var processedIncludeStatements = PreprocessInclude(includeStatements, includeFiles);
                outputStatements.AddRange(processedIncludeStatements);
            }

            return outputStatements;
        }

        private IEnumerator PreprocessAliasesAndVariables(List<string> statements)
        {
            foreach (var s in statements)
            {
                if (s.StartsWith("alias_text"))
                {
                    if (ParseStatement(s) is AliasText command)
                        yield return ExecuteSingle(command);
                }
                else if (s.StartsWith("set"))
                {
                    if (ParseStatement(s) is Set command)
                        command.Execute().InstantExecution();
                }
            }
        }

        #endregion
    }
}