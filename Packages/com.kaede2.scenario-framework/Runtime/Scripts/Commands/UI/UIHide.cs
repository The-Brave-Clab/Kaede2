using System.Collections;

namespace Kaede2.Scenario.Framework.Commands
{
    public abstract class UIShowHideBase : Command
    {
        public UIShowHideBase(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        protected abstract bool Show { get; }

        public override IEnumerator Execute()
        {
            Module.UIController.UICanvas.gameObject.SetActive(Show);
            yield break;
        }
    }

    public class UIHide : UIShowHideBase
    {
        public UIHide(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        protected override bool Show => false;
    }
}