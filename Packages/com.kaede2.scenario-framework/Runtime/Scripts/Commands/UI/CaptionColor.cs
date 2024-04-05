using System.Collections;
using Kaede2.Scenario.Framework.UI;

namespace Kaede2.Scenario.Framework.Commands
{
    public class CaptionColor : Command
    {
        private readonly UnityEngine.Color color;
        private readonly bool setDefault;

        private CaptionBox captionBox;

        public CaptionColor(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            color.r = Arg(2, 0);
            color.g = Arg(3, 0);
            color.b = Arg(4, 0);
            color.a = Arg(5, 1);
            setDefault = Arg(6, false);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override void Setup()
        {
            captionBox = Module.UIController.CaptionBox;
        }

        public override IEnumerator Execute()
        {
            if (setDefault)
            {
                Module.UIController.CaptionDefaultColor = color;
            }

            captionBox.box.color = color;
            yield break;
        }
    }
}