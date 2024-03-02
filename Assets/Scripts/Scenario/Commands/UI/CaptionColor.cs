using System.Collections;
using Kaede2.UI.ScenarioScene;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class CaptionColor : ScenarioModule.Command
    {
        private readonly Color color;
        private readonly bool setDefault;

        private CaptionState startState;
        private Color startDefaultColor;

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

        public override IEnumerator Setup()
        {
            startState = UIManager.Instance.CaptionBox.GetState();
            startDefaultColor = UIManager.Instance.CaptionDefaultColor;
            yield break;
        }

        public override IEnumerator Execute()
        {
            if (setDefault)
            {
                UIManager.Instance.CaptionDefaultColor = color;
            }
            
            UIManager.Instance.CaptionBox.box.color = color;
            yield break;
        }

        public override void Undo()
        {
            UIManager.Instance.CaptionBox.RestoreState(startState);
            // set default color regardless of the setDefault flag
            UIManager.Instance.CaptionDefaultColor = startDefaultColor;
        }
    }
}