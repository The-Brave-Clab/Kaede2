using System.Collections;
using System.Collections.Generic;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Kaede2
{
    public class MainStorySceneController : StorySelectionSceneController
    {
        [SerializeField]
        private string backgroundSpriteName;

        [SerializeField]
        private List<Image> backgroundImage;

        [SerializeField]
        private ChapterSelector chapterSelector;

        [SerializeField]
        private InterfaceTitle episodeSelectionTitle;

        [SerializeField]
        private OverlayBlend mainOverlay;

        [SerializeField]
        private OverlayBlend subOverlay;

        private AsyncOperationHandle<Sprite> backgroundHandle;

        private IEnumerator Start()
        {
            backgroundHandle = ResourceLoader.LoadIllustration(backgroundSpriteName);

            chapterSelector.SetSceneController(this);
            
            yield return backgroundHandle;
            foreach (var image in backgroundImage)
            {
                image.sprite = backgroundHandle.Result;
            }

            InitialSetup();

            yield return SceneTransition.Fade(0);
        }

        private void OnDestroy()
        {
            if (backgroundHandle.IsValid())
                Addressables.Release(backgroundHandle);
        }

        protected override void OnEnterEpisodeSelection(MasterScenarioInfo.IProvider provider)
        {
            if (provider is not MainStoryChapter chapter)
                return;

            episodeSelectionTitle.Text = chapter.Text.Replace('\n', ' ');
            mainOverlay.gameObject.SetActive(!chapter.IsSub);
            subOverlay.gameObject.SetActive(chapter.IsSub);
        }
    }
}