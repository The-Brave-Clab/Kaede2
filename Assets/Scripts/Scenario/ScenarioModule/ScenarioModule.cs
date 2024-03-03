using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Utils;
using NCalc;
using UnityEngine;

namespace Kaede2.Scenario
{
    public partial class ScenarioModule : Singleton<ScenarioModule>
    {
        public static string ScenarioName;

        private List<ResourceLoader.HandleBase> handles;
        private List<string> preprocessedStatements;

        public int StatementCount => preprocessedStatements.Count;

        [SerializeField]
        private List<GameObject> effectPrefabs;

#if UNITY_EDITOR
        [SerializeField]
        [Header("For editor only")]
        private string defaultScenarioName;
#endif

        // states
        public bool ActorAutoDelete { get; set; }
        public bool LipSync { get; set; }

        protected override void Awake()
        {
            base.Awake();

            scenarioResource = new();
            handles = new();
            preprocessedStatements = new();
            aliases = new();
            variables = new();
            commands = new();
            currentCommandIndex = -1;

            ActorAutoDelete = false;
            LipSync = true;
        }

        private IEnumerator Start()
        {

#if UNITY_EDITOR
            if (string.IsNullOrEmpty(ScenarioName))
            {
                // in editor we might directly run the scenario scene
                // in this case, we set a default scenario name
                ScenarioName = defaultScenarioName;

                // we might also need to do a global initialization here
                // since we have skipped the splash screen
                if (GlobalInitializer.CurrentStatus == GlobalInitializer.Status.NotStarted)
                    yield return GlobalInitializer.Initialize();
                else if (GlobalInitializer.CurrentStatus == GlobalInitializer.Status.InProgress)
                    yield return GlobalInitializer.Wait();
            }
#endif

            var scriptHandle = ResourceLoader.LoadScenarioScriptText(ScenarioName);
            // we could just release this right after getting the text string instead of releasing with other handles,
            // but it will usually unload the scenario bundle too which we are still going to use right after this
            // so we will release it with other handles
            handles.Add(scriptHandle);
            yield return scriptHandle.Send();

            var scriptAsset = scriptHandle.Result;
            var originalStatements = GetStatementsFromScript(scriptAsset.text);

            Dictionary<string, List<string>> includeFiles = new();
            yield return PreloadIncludeFiles(originalStatements, includeFiles);

            var includePreprocessedStatements = PreprocessInclude(originalStatements, includeFiles);
            preprocessedStatements = PreprocessFunctions(includePreprocessedStatements);
            yield return PreprocessAliasesAndVariables(preprocessedStatements);

            commands = preprocessedStatements.Select(ParseStatement).ToList();

            StartCoroutine(Execute());
        }

        private void OnDestroy()
        {
            foreach (var handle in handles)
            {
                handle.Dispose();
            }
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

        public string Statement(int index)
        {
            return preprocessedStatements[index];
        }
        #region Variables

        private Dictionary<string, Expression> variables;

        public void AddVariable(string variable, string value)
        {
            if (variable == value)
            {
                Debug.LogError("Variable cannot be equal to value");
                return;
            }

            variables[variable] = new Expression(value);
        }

        public T Evaluate<T>(string expression)
        {
            var exp = new Expression(expression);
            foreach (var v in variables)
            {
                exp.Parameters[v.Key] = v.Value;
            }

            var result = exp.Evaluate();
            return (T) Convert.ChangeType(result, typeof(T));
        }

#if UNITY_EDITOR
        private string ResolveExpression(string expression)
        {
            try
            {
                var exp = new Expression(expression);
                foreach (var v in variables)
                {
                    exp.Parameters[v.Key] = v.Value;
                }

                exp.Evaluate();
                return exp.Evaluate().ToString();
            }
            catch (Exception)
            {
                return expression;
            }
        }
#endif

        #endregion

        #region Alias

        private Dictionary<string, string> aliases;

        public void AddAlias(string orig, string alias)
        {
            aliases[alias] = orig;
        }

        public string ResolveAlias(string token)
        {
            if (aliases == null) return token;
            string result = token;
            var sortedKeys = aliases.Keys.ToList();
            sortedKeys.Sort((k2, k1) => k1.Length.CompareTo(k2.Length));
            while (true)
            {
                var replace = sortedKeys.Aggregate(result, 
                    (current, key) => 
                        current.Replace(key, aliases[key]));

                if (replace == result)
                    break;

                result = replace;
            }

            return result;
        }

        #endregion
    }
}