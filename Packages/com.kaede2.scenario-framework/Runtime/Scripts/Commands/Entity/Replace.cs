﻿using System.Collections;
using DG.Tweening;
using Kaede2.Scenario.Framework.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class Replace : Command
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

            if (!Module.ScenarioResource.Backgrounds.TryGetValue(resourceName, out var tex))
            {
                Debug.LogError($"Background texture {resourceName} not found");
                yield break;
            }

            UnityEngine.Color clearWhite = new(1, 1, 1, 0);

            Transform originalTransform = originalEntity.transform;
            originalEntity.gameObject.name = "_REPLACE_" + objName;

            var newEntity = Module.UIController.CreateBackground(objName, resourceName, tex);
            newEntity.SetColor(clearWhite);
            newEntity.transform.SetSiblingIndex(originalTransform.GetSiblingIndex() + 1);

            if (duration == 0)
            {
                newEntity.SetColor(UnityEngine.Color.white);
                Object.Destroy(originalEntity.gameObject);
                yield break;
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(DOVirtual.Color(clearWhite, UnityEngine.Color.white, duration,
                value => newEntity.SetColor(value)));
            seq.OnComplete(() => Object.Destroy(originalEntity.gameObject));

            yield return seq.WaitForCompletion();
        }
    }
}