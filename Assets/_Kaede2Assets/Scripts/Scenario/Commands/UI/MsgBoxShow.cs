using System.Collections;
using Kaede2.Scenario.Base;
using Kaede2.Scenario.UI;

namespace Kaede2.Scenario.Commands
{
    public abstract class MsgBoxShowHideBase : Command
    {
        private MessageBox messageBox;

        protected MsgBoxShowHideBase(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        protected abstract bool IsShow { get; }

        public override IEnumerator Setup()
        {
            messageBox = Module.UIController.MessageBox;
            yield break;
        }

        public override IEnumerator Execute()
        {
            messageBox.gameObject.SetActive(IsShow);
            messageBox.nameTag.text = "";
            messageBox.Text = "";
            yield break;
        }
    }

    public class MsgBoxShow : MsgBoxShowHideBase
    {
        public MsgBoxShow(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
        }

        protected override bool IsShow => true;
    }
}