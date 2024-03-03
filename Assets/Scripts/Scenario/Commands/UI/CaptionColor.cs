﻿using System.Collections;
using Kaede2.Scenario.UI;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class CaptionColor : ScenarioModule.Command
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

        public override IEnumerator Setup()
        {
            captionBox = UIManager.Instance.CaptionBox;
            yield break;
        }

        public override IEnumerator Execute()
        {
            if (setDefault)
            {
                UIManager.Instance.CaptionDefaultColor = color;
            }

            captionBox.box.color = color;
            yield break;
        }
    }
}