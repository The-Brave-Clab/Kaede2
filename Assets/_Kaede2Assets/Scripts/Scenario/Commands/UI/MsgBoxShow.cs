using System.Collections;
using Kaede2.Scenario.Base;
using Kaede2.Scenario.UI;

namespace Kaede2.Scenario.Commands
{
    public class MsgBoxShow : Command
    {
        private MessageBoxState startState;
        private MessageBox messageBox;

        public MsgBoxShow(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type { get; }
        public override float ExpectedExecutionTime { get; }

        public override IEnumerator Setup()
        {
            messageBox = Module.UIController.MessageBox;
            startState = messageBox.GetState();
            yield break;
        }

        public override IEnumerator Execute()
        {
            messageBox.gameObject.SetActive(true);
            yield break;
        }
    }
}