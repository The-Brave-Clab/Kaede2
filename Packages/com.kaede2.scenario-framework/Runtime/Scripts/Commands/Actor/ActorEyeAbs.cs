using System.Collections;
using Kaede2.Scenario.Framework.Entities;

namespace Kaede2.Scenario.Framework.Commands
{
    public class ActorEyeAbs : Command
    {
        private readonly string actorName;
        private readonly float value;

        private Live2DActorEntity entity;

        public ActorEyeAbs(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            actorName = OriginalArg(1);
            value = Arg(2, 0.0f);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Setup()
        {
            FindEntity(actorName, out entity);
            yield break;
        }

        public override IEnumerator Execute()
        {
            entity.AddEyeX = 0;
            entity.AbsoluteEyeX = value;
            yield break;
        }
    }
}