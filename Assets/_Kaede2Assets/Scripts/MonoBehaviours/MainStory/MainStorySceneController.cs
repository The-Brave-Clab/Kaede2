using System.Collections;
using System.Collections.Generic;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Kaede2
{
    public class MainStorySceneController : StorySelectionSceneController
    {
        [SerializeField]
        private AssetReferenceSprite backgroundSprite;

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

        private IEnumerator Start()
        {
            var handle = backgroundSprite.LoadAssetAsync<Sprite>();

            chapterSelector.SetSceneController(this);
            
            yield return handle;
            foreach (var image in backgroundImage)
            {
                image.sprite = handle.Result;
            }

            InitialSetup();

            yield return SceneTransition.Fade(0);
        }

        private void OnDestroy()
        {
            if (backgroundSprite.IsValid())
                backgroundSprite.ReleaseAsset();
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