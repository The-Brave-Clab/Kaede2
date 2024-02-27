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

        protected override void Awake()
        {
            base.Awake();

            handles = new();
        }

        private IEnumerator Start()
        {
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