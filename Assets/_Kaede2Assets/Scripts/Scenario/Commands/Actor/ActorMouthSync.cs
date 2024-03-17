using System.Collections;
using Kaede2.Scenario.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class ActorMouthSync : Command
    {
        private readonly string slaveName;
        private readonly string masterName;

        private Live2DActorEntity slave;
        private Live2DActorEntity master;

        public ActorMouthSync(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            slaveName = OriginalArg(1);
            masterName = OriginalArg(2);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Setup()
        {
            FindEntity(slaveName, out slave);
            FindEntity(masterName, out master);
            yield break;
        }

        public override IEnumerator Execute()
        {
            if (master == null)
            {
                Debug.LogError($"Live2D Actor Entity {masterName} not found");
                yield break;
            }

            if (slave == null)
            {
                Debug.LogError($"Live2D Actor Entity {slaveName} not found");
                yield break;
            }

            master.AddMouthSync(slave);
        }
    }
}