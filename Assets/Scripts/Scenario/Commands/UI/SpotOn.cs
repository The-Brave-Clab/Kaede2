using System.Collections;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class SpotOn : ScenarioModule.Command
    {
        private readonly string targetName;

        private ScenarioModule.Entity targetEntity;
        private ScenarioModule.Entity[] allEntities;

        public SpotOn(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            targetName = OriginalArg(1);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Setup()
        {
            FindEntity(targetName, out targetEntity);
            allEntities = Object.FindObjectsByType<ScenarioModule.Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            yield break;
        }

        public override IEnumerator Execute()
        {
            foreach (var entity in allEntities)
            {
                var alpha = entity.GetColor().a;
                entity.SetColor(new Color(0.3f, 0.3f, 0.3f, alpha));
            }
            var alpha2 = targetEntity.GetColor().a;
            targetEntity.SetColor(new Color(1.0f, 1.0f , 1.0f, alpha2));

            yield break;
        }
    }
}