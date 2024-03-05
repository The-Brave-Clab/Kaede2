using System.Collections;
using Kaede2.Scenario.UI;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class InitEnd : ScenarioModule.Command
    {
        public InitEnd(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Execute()
        {
            UIManager.Instance.loadingCanvas.gameObject.SetActive(false);
            Module.Initialized = true;
            Debug.Log("Scenario initialized");
            if (ScenarioModule.SyncPointToBeRestored != null)
            {
                Debug.Log("Restoring sync point");
                Module.RestoreState(ScenarioModule.SyncPointToBeRestored.Value);
                ScenarioModule.SyncPointToBeRestored = null;
            }
            yield break;
        }
    }
}