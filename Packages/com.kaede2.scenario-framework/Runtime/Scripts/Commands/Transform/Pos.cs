using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public abstract class PosBase : Command
    {
        private readonly string entityName;
        protected readonly Vector2 Position;
        private readonly float duration;
        private readonly Ease ease;
        private readonly bool wait;

        protected Entity Entity;

        protected abstract Vector3 TargetPos { get; }

        protected PosBase(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            entityName = OriginalArg(1);
            Position = new Vector2(Arg(2, 0.0f), Arg(3, 0.0f));
            duration = Arg(4, 0.0f);
            ease = Arg(5, Ease.Linear);
            wait = Arg(6, true);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, ExpectedExecutionTime);
        public override float ExpectedExecutionTime => duration;

        public override IEnumerator Setup()
        {
            FindEntity(entityName, out Entity);
            yield break;
        }

        public override IEnumerator Execute()
        {
            if (Entity == null)
            {
                Debug.LogError($"Entity {entityName} not found");
                yield break;
            }

            if (duration == 0)
            {
                Entity.Position = TargetPos;
                yield break;
            }

            Entity.EnsureMoveStopped();
            yield return Entity.Move(Entity.Position, TargetPos, duration, ease);
        }
    }

    public class Pos : PosBase
    {
        public Pos(ScenarioModule module, string[] arguments) : base(module, arguments) { }

        protected override Vector3 TargetPos => Position;
    }
}