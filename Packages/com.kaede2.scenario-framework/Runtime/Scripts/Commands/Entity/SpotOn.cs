using System.Collections;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class SpotOn : Command
    {
        private readonly string targetName;

        private Entity targetEntity;
        private Entity[] allEntities;

        public SpotOn(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            targetName = OriginalArg(1);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Setup()
        {
            FindEntity(targetName, out targetEntity);
            allEntities = Object.FindObjectsByType<Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            yield break;
        }

        public override IEnumerator Execute()
        {
            foreach (var entity in allEntities)
            {
                var alpha = entity.GetColor().a;
                entity.SetColor(new UnityEngine.Color(0.6f, 0.6f, 0.6f, alpha));
            }
            var alpha2 = targetEntity.GetColor().a;
            targetEntity.SetColor(new UnityEngine.Color(1.0f, 1.0f , 1.0f, alpha2));

            yield break;
        }
    }
}