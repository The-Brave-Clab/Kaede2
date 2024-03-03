using System.Collections;
using Kaede2.Scenario.UI;

namespace Kaede2.Scenario.Commands
{
    public class UIShow : ScenarioModule.Command
    {
        public UIShow(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Execute()
        {
            UIManager.Instance.uiCanvas.gameObject.SetActive(true);
            yield break;
        }
    }
}