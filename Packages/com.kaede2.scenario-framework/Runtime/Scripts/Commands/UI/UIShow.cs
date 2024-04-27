using System.Collections;

namespace Kaede2.Scenario.Framework.Commands
{
    public class UIShow : UIShowHideBase
    {
        public UIShow(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        protected override bool Show => true;
    }
}