using Kaede2.Scenario.Base;

namespace Kaede2.Scenario.Commands
{
    public class MsgBoxHide : MsgBoxShowHideBase
    {
        public MsgBoxHide(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
        }

        protected override bool IsShow => false;
    }
}
