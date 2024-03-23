using System.Collections;

namespace Kaede2.Scenario.Framework.Commands
{
    public class UIHide : Command
    {
        public UIHide(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Execute()
        {
            Module.UIController.UICanvas.gameObject.SetActive(false);
            yield break;
        }
    }
}