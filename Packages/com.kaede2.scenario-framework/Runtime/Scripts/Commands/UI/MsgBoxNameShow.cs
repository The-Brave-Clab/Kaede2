using System.Collections;
using Kaede2.Scenario.Framework.UI;

namespace Kaede2.Scenario.Framework.Commands
{
    public class MsgBoxNameShow : Command
    {
        private readonly bool show;
        private readonly string key;

        private MessageBox messageBox;

        public MsgBoxNameShow(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            show = Arg(1, true);
            key = Arg(2, "__DEFAULT_PANE__"); // not used
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override void Setup()
        {
            messageBox = Module.UIController.MessageBox;
        }

        public override IEnumerator Execute()
        {
            messageBox.NamePanelEnabled = show;
            yield break;
        }
    }
}