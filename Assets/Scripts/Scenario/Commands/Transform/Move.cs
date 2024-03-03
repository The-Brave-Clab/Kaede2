using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class Move : PosBase
    {
        public Move(ScenarioModule module, string[] arguments) : base(module, arguments) { }

        protected override Vector3 TargetPos => Entity.Position + (Vector3)Position;
    }
}