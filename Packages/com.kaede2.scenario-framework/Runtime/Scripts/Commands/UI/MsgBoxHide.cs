namespace Kaede2.Scenario.Framework.Commands
{
    public class MsgBoxHide : MsgBoxShowHideBase
    {
        public MsgBoxHide(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        protected override bool IsShow => false;
    }
}
