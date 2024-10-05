using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kaede2.Scenario;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kaede2
{
    public class MainStorySceneController : StorySelectionSceneController
    {
        [SerializeField]
        private AssetReferenceSprite backgroundSprite;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private ChapterSelector chapterSelector;

        [SerializeField]
        private InterfaceTitle episodeSelectionTitle;

        private IEnumerator Start()
        {
            var handle = backgroundSprite.LoadAssetAsync<Sprite>();

            chapterSelector.SetSceneController(this);
            
            yield return handle;
            backgroundImage.sprite = handle.Result;

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

            episodeSelectionTitle.Text = chapter.Text;
        }
    }
}