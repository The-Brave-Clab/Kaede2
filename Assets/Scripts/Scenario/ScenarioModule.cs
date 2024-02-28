using System;
using System.Collections;
using System.Collections.Generic;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2.Scenario
{
    public class ScenarioModule : Singleton<ScenarioModule>
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
            Debug.Log(scriptAsset.text);
        }

        private void OnDestroy()
        {
            foreach (var handle in handles)
            {
                handle.Dispose();
            }
        }
    }
}