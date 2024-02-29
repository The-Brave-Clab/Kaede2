using System.Collections;
using System.Collections.Generic;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2.Scenario
{
    public partial class ScenarioModule : Singleton<ScenarioModule>
    {
        public static string ScenarioName;

        private List<ResourceLoader.HandleBase> handles;

#if UNITY_EDITOR
        [SerializeField]
        [Header("For editor only")]
        private string defaultScenarioName;
#endif

        protected override void Awake()
        {
            base.Awake();

            handles = new();
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
            var finalStatements = PreprocessFunctions(includePreprocessedStatements);

            Debug.Log(string.Join("\n", finalStatements));
        }

        private void OnDestroy()
        {
            foreach (var handle in handles)
            {
                handle.Dispose();
            }
        }

        public static List<string> GetStatementsFromScript(string script)
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
    }
}