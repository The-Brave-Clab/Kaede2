﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Input;
using Kaede2.Scenario.Audio;
using Kaede2.Scenario.Commands;
using Kaede2.Scenario.UI;
using Kaede2.Utils;
using NCalc;
using UnityEngine;

namespace Kaede2.Scenario
{
    public class ScenarioModule : ScenarioModuleBase
    {
        public static string GlobalScenarioName;
        public static ScenarioState StateToBeRestored;

        private List<string> statements;
        private List<Command> commands;
        private int currentCommandIndex;

        [SerializeField]
        private UIManager uiManager;

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

        public override int CurrentCommandIndex
        {
            get => currentCommandIndex;
            protected set => currentCommandIndex = value;
        }

        public override UIManager UIManager => uiManager;
        public override AudioManager AudioManager => audioManager;

        public override void InitEnd()
        {
            UIManager.loadingCanvas.gameObject.SetActive(false);
            Debug.Log("Scenario initialized");
            if (StateToBeRestored != null)
            {
                Debug.Log("Restoring sync point");
                RestoreState(StateToBeRestored);
                StateToBeRestored = null;
            }
        }

        public override void End()
        {
            Debug.Log("Scenario ended");
        }

        protected override void Awake()
        {
            base.Awake();

            statements = new();
            commands = new();
            currentCommandIndex = -1;

            InputManager.InputAction.Scenario.Enable();
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
            RegisterLoadHandle(scriptHandle);
            yield return scriptHandle.Send();

            var scriptAsset = scriptHandle.Result;
            var originalStatements = GetStatementsFromScript(scriptAsset.text);

            Dictionary<string, List<string>> includeFiles = new();
            yield return PreloadIncludeFiles(originalStatements, includeFiles);

            var includePreprocessedStatements = PreprocessInclude(originalStatements, includeFiles);
            statements = PreprocessFunctions(includePreprocessedStatements);
            yield return PreprocessAliasesAndVariables(statements);

            commands = statements.Select(ParseStatement).ToList();

            StartCoroutine(Execute());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (InputManager.Instance != null)
                InputManager.InputAction.Scenario.Disable();
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