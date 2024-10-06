using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2
{
    public class CollabStoryController : StorySelectionSceneController
    {
        [SerializeField]
        private RandomizeScatterImages randomizeScatterImages;

        [SerializeField]
        private StoryCategorySelectable[] storyCategorySelectables;

        [SerializeField]
        private Canvas randomizedImageBackgroundCanvas;

        [SerializeField]
        private StorySelectionBackground storySelectionBackground;

        [SerializeField]
        private AlbumExtraInfo albumExtraInfo;

        [SerializeField]
        private CollabContentController collabContent;

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

            InitialSetup();

            yield return SceneTransition.Fade(0);
        }

        public void EnterCollabContent(CollabProvider collab)
        {
            CoroutineProxy.Start(EnterCollabContentCoroutine(collab));
        }

        private IEnumerator EnterCollabContentCoroutine(CollabProvider collab)
        {
            yield return SceneTransition.Fade(1);

            gameObject.SetActive(false);
            randomizedImageBackgroundCanvas.gameObject.SetActive(false);
            collabContent.gameObject.SetActive(true);
            storySelectableGroup.transform.parent.gameObject.SetActive(false);
            selectionCanvas.gameObject.SetActive(true);

            collabContent.Initialize(collab);

            yield return SceneTransition.Fade(0);
        }

        public void ExitCollabContent()
        {
            CoroutineProxy.Start(ExitCollabContentCoroutine());
        }

        private IEnumerator ExitCollabContentCoroutine()
        {
            yield return SceneTransition.Fade(1);

            collabContent.gameObject.SetActive(false);
            gameObject.SetActive(true);
            randomizedImageBackgroundCanvas.gameObject.SetActive(true);

            yield return SceneTransition.Fade(0);
        }

        protected override void InitialSetup()
        {
            base.InitialSetup();
            collabContent.gameObject.SetActive(true);
        }

        private class CollabStoryProvider : MasterScenarioInfo.IProvider
        {
            private readonly MasterCollabInfo.CollabType collabType;
            private readonly MasterCollabInfo.CollabInfo collabInfo;

            public string Label => "コラボ"; // just hardcode it, it's fine
            public string Title => collabInfo?.CollabName;

            public CollabStoryProvider(MasterCollabInfo.CollabType type)
            {
                collabType = type;
                collabInfo = MasterCollabInfo.Instance.Data
                    .FirstOrDefault(ci => ci.CollabType == collabType);
            }

            public IEnumerable<MasterScenarioInfo.ScenarioInfo> Provide()
            {
                if (collabInfo != null)
                    return MasterScenarioInfo.Instance.Data
                        .Where(si => si.EpisodeId == collabInfo.StoryEpisodeId);

                // this should not happen but just in case
                this.LogError($"Collab info not found for {collabType:G}");
                return Array.Empty<MasterScenarioInfo.ScenarioInfo>();
            }
        }
    }

}
