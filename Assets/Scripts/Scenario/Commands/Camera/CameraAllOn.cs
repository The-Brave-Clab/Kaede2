using System.Collections;
using Kaede2.Scenario.UI;

namespace Kaede2.Scenario.Commands
{
    public class CameraAllOn : ScenarioModule.Command
    {
        public CameraAllOn(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Execute()
        {
            UIManager.Instance.contentCanvas.gameObject.SetActive(true);
            yield break;
        }
    }
}