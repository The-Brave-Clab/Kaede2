using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;

namespace Kaede2
{
    public class CartoonSceneController : MonoBehaviour
    {
        [SerializeField]
        private RandomizeScatterImages scatterImages;

        [SerializeField]
        private CartoonChapterSelection cartoonFramePrefab;

        [SerializeField]
        private Canvas chapterSelectionCanvas;

        [SerializeField]
        private RectTransform chapterFrameParent;

        [SerializeField]
        private CartoonEpisodeSelection episodeSelection;

        private class CartoonInfoGroupIdComparer : IEqualityComparer<MasterCartoonInfo.CartoonInfo>
        {
            public bool Equals(MasterCartoonInfo.CartoonInfo x, MasterCartoonInfo.CartoonInfo y)
            {
                return x?.GroupId == y?.GroupId;
            }

            public int GetHashCode(MasterCartoonInfo.CartoonInfo obj)
            {
                return obj.GroupId.GetHashCode();
            }
        }

        private void Awake()
        {
            episodeSelection.gameObject.SetActive(false);
            chapterSelectionCanvas.gameObject.SetActive(true);
        }

        private IEnumerator Start()
        {
            var cartoonChapters = MasterCartoonInfo.Instance.cartoonInfo
                .Distinct(new CartoonInfoGroupIdComparer())
                .OrderBy(ci => ci.No)
                .ToList();

            var group = new CoroutineGroup();

            group.Add(new WaitUntil(() => scatterImages.Loaded));

            for (int i = 0; i < cartoonChapters.Count; i++)
            {
                var chapter = cartoonChapters[i];
                var chapterSelection = Instantiate(cartoonFramePrefab, chapterFrameParent);
                group.Add(chapterSelection.Initialize(this, i + 1));
            }

            yield return group.WaitForAll();

            yield return SceneTransition.Fade(0);
        }

        public void OnChapterSelected(CartoonChapterSelection chapterSelection)
        {
            StartCoroutine(OnChapterSelectedCoroutine(chapterSelection));
        }

        private IEnumerator OnChapterSelectedCoroutine(CartoonChapterSelection chapterSelection)
        {
            yield return SceneTransition.Fade(1);

            episodeSelection.gameObject.SetActive(true);
            chapterSelectionCanvas.gameObject.SetActive(false);

            yield return episodeSelection.Initialize(chapterSelection);

            yield return SceneTransition.Fade(0);
        }
    }
}
