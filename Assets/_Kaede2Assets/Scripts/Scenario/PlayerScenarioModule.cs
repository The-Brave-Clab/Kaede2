using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Input;
using Kaede2.Scenario.Framework;
using Kaede2.Scenario.Framework.Commands;
using Kaede2.Scenario.Framework.Utils;
#if UNITY_WEBGL && !UNITY_EDITOR
using Kaede2.UI.Web;
#endif
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2.Scenario
{
    public class PlayerScenarioModule : ScenarioModule
    {
        public static string GlobalScenarioName;
        public static ScenarioState StateToBeRestored;

        private List<string> statements;
        private List<Command> commands;
        private int currentCommandIndex;

        private List<ResourceLoader.HandleBase> resourceHandles;

        [SerializeField]
        private UIController uiController;

        [SerializeField]
        private AudioManager audioManager;

#if UNITY_EDITOR
        [Header("For editor only")]
        public string defaultScenarioName;
#endif

        public override string ScenarioName
        {
            get
            {
#if UNITY_EDITOR
                if (string.IsNullOrEmpty(GlobalScenarioName))
                {
                    // in editor we might directly run the scenario scene
                    // in this case, we set a default scenario name
                    return defaultScenarioName;
                }
#endif
                return GlobalScenarioName;
            }
        }

        public override IReadOnlyList<string> Statements => statements.AsReadOnly();
        public override IReadOnlyList<Command> Commands => commands.AsReadOnly();

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

        public override bool MesClicked => InputManager.InputAction.Scenario.Next.triggered;

        public override UIController UIController => uiController;
        public override AudioManager AudioManager => audioManager;

        protected override void Awake()
        {
            base.Awake();

            statements = new();
            commands = new();
            currentCommandIndex = -1;

            resourceHandles = new();

            InputManager.InputAction.Scenario.Enable();
#if UNITY_WEBGL && !UNITY_EDITOR
            WebInterop.Module = this;
            OnMesCommand += WebInterop.OnMessageCommand;
#endif
        }

        private IEnumerator Start()
        {
#if UNITY_EDITOR
            // we might want to do a global initialization here
            // since we might skip the splash screen
            if (GlobalInitializer.CurrentStatus != GlobalInitializer.Status.Done)
                yield return GlobalInitializer.Initialize();
#endif

            var scriptHandle = ResourceLoader.LoadScenarioScriptText(ScenarioName);
            // we could just release this right after getting the text string instead of releasing with other handles,
            // but it will usually unload the scenario bundle too which we are still going to use right after this
            // so we will release it with other handles
            resourceHandles.Add(scriptHandle);
            yield return scriptHandle.Send();

            var scriptAsset = scriptHandle.Result;
#if UNITY_WEBGL && !UNITY_EDITOR
            WebInterop.OnScriptLoaded(scriptAsset.text);
#endif
            var originalStatements = GetStatementsFromScript(scriptAsset.text);

            Dictionary<string, List<string>> includeFiles = new();
            yield return PreloadIncludeFiles(originalStatements, includeFiles);

            var includePreprocessedStatements = PreprocessInclude(originalStatements, includeFiles);
            statements = PreprocessFunctions(includePreprocessedStatements);
            yield return PreprocessAliasesAndVariables(statements);

            commands = statements.Select(ParseStatement).ToList();

            StartCoroutine(Execute());
        }

        protected void OnDestroy()
        {
            foreach (var handle in resourceHandles)
            {
                handle.Dispose();
            }

            if (InputManager.Instance != null)
                InputManager.InputAction.Scenario.Disable();

#if UNITY_WEBGL && !UNITY_EDITOR
            WebInterop.Module = null;
#endif
        }

        public override IEnumerator InitEnd()
        {
            UIController.LoadingCanvas.gameObject.SetActive(false);
            Debug.Log("Scenario initialized");
            if (StateToBeRestored != null)
            {
                Debug.Log("Restoring sync point");
                RestoreState(StateToBeRestored);
                StateToBeRestored = null;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            while (WebBackground.CurrentStatus == WebBackground.Status.ReadyToPlay)
                yield return null;
            WebInterop.OnScenarioStarted();
#endif

            yield break;
        }

        public override IEnumerator End()
        {
            Debug.Log("Scenario ended");
#if UNITY_WEBGL && !UNITY_EDITOR
            WebBackground.UpdateStatus(WebBackground.Status.Finished);
            WebInterop.OnScenarioFinished();
#endif
            yield break;
        }

        private IEnumerator SendHandleWithFinishCallback<T>(ResourceLoader.LoadAddressableHandle<T> handle, Action<T> callback) where T : UnityEngine.Object
        {
            yield return handle.Send();
            if (handle.Result == null)
            {
                Debug.LogError($"Failed to load asset {handle.AssetAddress}");
            }

            callback(handle.Result);
        }

        private IEnumerator SendLive2DHandleWithFinish(ResourceLoader.LoadLive2DHandle handle, string resourceName)
        {
            yield return handle.Send();
            if (handle.Result == null)
            {
                Debug.LogError($"Failed to Live2D model {resourceName}");
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

                Debug.Log($"Pre-Loaded include file {fileName}");
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

        private static List<string> PreprocessFunctions(List<string> statements)
        {
            Dictionary<string, Function> functions = new();
            Function currentFunction = null;
            bool recordingFunction = false;

            List<string> outputStatements = new List<string>();

            foreach (var s in statements)
            {
                // if recording function, just add to current function
                if (recordingFunction && !s.StartsWith("endfunction"))
                {
                    currentFunction.AddStatement(s);
                    continue;
                }

                // if recording and we should end, finish recording
                if (recordingFunction && s.StartsWith("endfunction"))
                {
                    currentFunction.FinishDefinition();
                    functions.Add(currentFunction.FunctionName, currentFunction);

                    currentFunction = null;
                    recordingFunction = false;
                    continue;
                }

                // if not recording and we should start, start recording
                if (s.StartsWith("function"))
                {
                    currentFunction = new Function(s);
                    recordingFunction = true;
                    continue;
                }

                // if not recording and we should call a function, call it
                if (s.StartsWith("sub"))
                {
                    var split = s.Split('\t');
                    var functionName = split[1];
                    var parameters = new List<string>(split.Length - 2);
                    for (int i = 2; i < split.Length; ++i)
                    {
                        parameters.Add(split[i]);
                    }

                    if (!functions.ContainsKey(functionName))
                    {
                        Debug.LogError($"Function {functionName} doesn't exist!");
                        continue;
                    }

                    Function f = functions[functionName];
                    var functionStatements = f.GetStatements(parameters);

                    outputStatements.AddRange(functionStatements);
                    continue;
                }

                // if not recording and we should do something else, just add it
                outputStatements.Add(s);
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
    }
}