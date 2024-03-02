using System.Collections;
using Kaede2.UI.ScenarioScene;

namespace Kaede2.Scenario.Commands
{
    public class MsgBoxHide : ScenarioModule.Command
    {
        private MessageBoxState startState;
        private MessageBox messageBox;

        public MsgBoxHide(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Setup()
        {
            messageBox = UIManager.Instance.MessageBox;
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

        public override void Undo()
        {
            messageBox.RestoreState(startState);
        }
    }
}
