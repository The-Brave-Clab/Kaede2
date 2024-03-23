using System.Collections;
using Kaede2.Scenario.Framework.UI;

namespace Kaede2.Scenario.Framework.Commands
{
    public abstract class MsgBoxShowHideBase : Command
    {
        private MessageBox messageBox;

        protected MsgBoxShowHideBase(ScenarioModule module, string[] arguments) : base(module, arguments)
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
        public MsgBoxShow(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        protected override bool IsShow => true;
    }
}