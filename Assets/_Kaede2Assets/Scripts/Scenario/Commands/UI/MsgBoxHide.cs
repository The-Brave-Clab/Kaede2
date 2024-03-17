using System.Collections;
using Kaede2.Scenario.UI;

namespace Kaede2.Scenario.Commands
{
    public class MsgBoxHide : Command
    {
        private MessageBoxState startState;
        private MessageBox messageBox;

        public MsgBoxHide(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Setup()
        {
            messageBox = Module.UIManager.MessageBox;
            startState = messageBox.GetState();
            yield break;
        }

        public override IEnumerator Execute()
        {
            messageBox.gameObject.SetActive(false);
            messageBox.nameTag.text = "";
            messageBox.SetText("");
            yield break;
        }
    }
}
