using System.Collections;
using Kaede2.Scenario.UI;

namespace Kaede2.Scenario.Commands
{
    public class UIHide : Command
    {
        public UIHide(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Execute()
        {
            Module.UIManager.uiCanvas.gameObject.SetActive(false);
            yield break;
        }
    }
}