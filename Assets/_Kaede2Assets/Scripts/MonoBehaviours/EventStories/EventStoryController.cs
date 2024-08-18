using System;
using System.Collections;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.UI;
using UnityEngine;

namespace Kaede2
{
    public class EventStoryController : MonoBehaviour
    {
        [SerializeField]
        private RandomizeScatterImages randomizeScatterImages;

        [SerializeField]
        private StoryCategorySelectable[] storyCategorySelectables;

        private IEnumerator Start()
        {
            IEnumerator WaitForCondition(Func<bool> condition)
            {
                while (!condition())
                    yield return null;
            }

            CoroutineGroup group = new CoroutineGroup();

            group.Add(WaitForCondition(() => randomizeScatterImages.Loaded));
            foreach (var selectable in storyCategorySelectables)
            {
                group.Add(WaitForCondition(() => selectable.Loaded));
            }

            yield return group.WaitForAll();

            yield return SceneTransition.Fade(0);
        }
    }

}
