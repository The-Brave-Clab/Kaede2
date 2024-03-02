using System.Collections;
using Kaede2.Scenario.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public abstract class ActorShowHideBase : ScenarioModule.Command
    {
        private readonly string actorName;
        private readonly float duration;
        private readonly bool wait;

        private Live2DActorEntity entity;

        protected abstract bool Hide { get; }

        public ActorShowHideBase(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            actorName = OriginalArg(1);
            duration = Arg(2, 0.0f);
            wait = Arg(3, true);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, duration);
        public override float ExpectedExecutionTime => duration;

        public override IEnumerator Setup()
        {
            FindEntity(actorName, out entity);
            yield break;
        }

        public override IEnumerator Execute()
        {
            if (entity == null)
            {
                Debug.LogError($"Live2D Actor Entity {actorName} not found");
                yield break;
            }

            if (duration > 0)
            {
                yield return new WaitForSeconds(duration);
            }

            entity.Hidden = Hide;
            if (Hide)
                entity.transform.eulerAngles = Vector3.zero;

            if (Module.ActorAutoDelete)
            {
                Object.Destroy(entity.gameObject);
            }
        }
    }

    public class ActorHide : ActorShowHideBase
    {
        public ActorHide(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        protected override bool Hide => true;
    }
}