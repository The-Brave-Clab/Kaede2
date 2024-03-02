using System.Collections;
using Kaede2.UI.ScenarioScene;

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
            yield break;
        }

        public override void Undo()
        {
        }
    }
}