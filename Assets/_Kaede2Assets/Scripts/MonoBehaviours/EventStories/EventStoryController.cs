using System;
using System.Collections;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
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

        [SerializeField]
        private LabeledListSelectableGroup chapterSelectableGroup;

        [SerializeField]
        private Canvas randomizedImageBackgroundCanvas;

        [SerializeField]
        private Canvas chapterSelectionCanvas;

        [SerializeField]
        private InterfaceTitle birthdayTitle;

        [SerializeField]
        private InterfaceTitle eventTitle;

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

            chapterSelectionCanvas.gameObject.SetActive(false);

            yield return SceneTransition.Fade(0);
        }

        public void EnterChapterSelection(bool isBirthday)
        {
            CoroutineProxy.Start(EnterChapterSelectionCoroutine(isBirthday ? MasterScenarioInfo.Kind.Birthday : MasterScenarioInfo.Kind.Event));
        }

        private IEnumerator EnterChapterSelectionCoroutine(MasterScenarioInfo.Kind kind)
        {
            yield return SceneTransition.Fade(1);

            gameObject.SetActive(false);
            randomizedImageBackgroundCanvas.gameObject.SetActive(false);
            chapterSelectionCanvas.gameObject.SetActive(true);

            if (kind == MasterScenarioInfo.Kind.Birthday)
            {
                birthdayTitle.gameObject.SetActive(true);
                eventTitle.gameObject.SetActive(false);
            }
            else if (kind == MasterScenarioInfo.Kind.Event)
            {
                birthdayTitle.gameObject.SetActive(false);
                eventTitle.gameObject.SetActive(true);
            }

            chapterSelectableGroup.Initialize(scenarioInfo => scenarioInfo.KindId == kind);

            yield return SceneTransition.Fade(0);
        }
    }

}
