using System.Collections;
using Kaede2.UI.ScenarioScene;

namespace Kaede2.Scenario.Commands
{
    public class MsgBoxShow : ScenarioModule.Command
    {
        private MessageBoxState startState;
        private MessageBox messageBox;

        public MsgBoxShow(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type { get; }
        public override float ExpectedExecutionTime { get; }

        public override IEnumerator Setup()
        {
            messageBox = UIManager.Instance.MessageBox;
            startState = messageBox.GetState();
            yield break;
        }

        public override IEnumerator Execute()
        {
            messageBox.gameObject.SetActive(true);
            yield break;
        }

        public override void Undo()
        {
            messageBox.RestoreState(startState);
        }
    }
}