using System.Collections;
using DG.Tweening;
using Kaede2.Scenario.Entities;
using Kaede2.Scenario.UI;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class Replace : ScenarioModule.Command
    {
        private readonly string resourceName;
        private readonly string objName;
        private readonly float duration;
        private readonly bool wait;

        private BackgroundEntity originalEntity;
        public Replace(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            resourceName = Arg(1, "");
            objName = OriginalArg(2);
            duration = Arg(3, 0.0f);
            wait = Arg(4, true);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, duration);
        public override float ExpectedExecutionTime => duration;

        public override IEnumerator Setup()
        {
            FindEntity(objName, out originalEntity);
            yield break;
        }

        public override IEnumerator Execute()
        {
            if (originalEntity == null)
            {
                Debug.LogError($"Background Entity {objName} not found");
                yield break;
            }

            if (!Module.ScenarioResource.backgrounds.TryGetValue(resourceName, out var tex))
            {
                Debug.LogError($"Background texture {resourceName} not found");
                yield break;
            }

            Color clearWhite = new(1, 1, 1, 0);

            Transform originalTransform = originalEntity.transform;
            originalEntity.gameObject.name = "_REPLACE_" + objName;

            var newBG = Object.Instantiate(originalTransform.gameObject, originalTransform.parent);
            var newEntity = newBG.GetComponent<BackgroundEntity>();
            newBG.name = objName;
            newEntity.resourceName = resourceName;
            newEntity.SetColor(clearWhite);
            newEntity.SetImage(tex);
            newEntity.transform.SetSiblingIndex(originalTransform.GetSiblingIndex() + 1);

            if (duration == 0)
            {
                newEntity.SetColor(Color.white);
                Object.Destroy(originalEntity.gameObject);
                yield break;
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(DOVirtual.Color(clearWhite, Color.white, duration,
                value => newEntity.SetColor(value)));
            seq.OnComplete(() => Object.Destroy(originalEntity.gameObject));

            yield return seq.WaitForCompletion();
        }
    }
}