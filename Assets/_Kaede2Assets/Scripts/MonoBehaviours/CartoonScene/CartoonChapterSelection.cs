using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Kaede2
{
    public class CartoonChapterSelection : MonoBehaviour, IThemeChangeObserver, IPointerEnterHandler
    {
        [SerializeField]
        private Image thumbnail;

        [SerializeField]
        private Image selectionFrame;

        [SerializeField]
        private ColorAdjustmentMask colorAdjustmentOverlay;

        [SerializeField]
        private List<TextMeshProUGUI> titleTexts;

        [SerializeField]
        private List<TextMeshProUGUI> chapterNumberTexts;

        [SerializeField]
        private int index;

        private AsyncOperationHandle<Sprite> thumbnailHandle;
        private MasterCartoonInfo.CartoonInfo cartoonChapter;

        private Coroutine selectionCoroutine;
        private Sequence selectionSequence;

        private static CartoonChapterSelection currentSelected = null;

        // it seems that we can't rely on TMP's line wrapping
        private static readonly List<int> splitLineIndices = new()
        {
            7, 6, 6, 5, 8, 4, 4, -1, 8, -1, 5, 4, -1, -1, 5, -1, 5, 4
        };

        private void Awake()
        {
            OnThemeChange(Theme.Current);
        }

        private IEnumerator Start()
        {
            // start with deselected state
            colorAdjustmentOverlay.gameObject.SetActive(true);
            colorAdjustmentOverlay.Saturation = -0.5f;
            var newColor = selectionFrame.color;
            newColor.a = 0;
            selectionFrame.color = newColor;

            yield return Initialize(index);
        }

        private void OnDestroy()
        {
            if (thumbnailHandle.IsValid())
                Addressables.Release(thumbnailHandle);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            var newColor = theme.CommonButtonColor.surface;
            newColor.a = selectionFrame.color.a;
            selectionFrame.color = newColor;
        }

        public IEnumerator Initialize(int cartoonChapterNumber) // note: starts from 1
        {
            cartoonChapter = MasterCartoonInfo.Instance.cartoonInfo
                .OrderBy(ci => ci.No)
                .FirstOrDefault(ci => ci.GroupId == $"#{cartoonChapterNumber}");

            if (cartoonChapter == null || cartoonChapterNumber - 1 > splitLineIndices.Count)
            {
                this.LogError($"Cartoon chapter #{cartoonChapterNumber} not found!");
                yield break;
            }

            thumbnailHandle = ResourceLoader.LoadCartoonFrame(cartoonChapter.ImageNames.FirstOrDefault());

            var groupTitle = cartoonChapter.GroupTitle;
            if (splitLineIndices[cartoonChapterNumber - 1] > 0)
            {
                groupTitle = groupTitle.Insert(splitLineIndices[cartoonChapterNumber - 1], "\n");
            }
            // also we want to replace full-width exclamation mark with half-width one
            groupTitle = groupTitle.Replace("！", "!");

            foreach (var text in titleTexts)
            {
                text.text = groupTitle;
            }

            foreach (var text in chapterNumberTexts)
            {
                text.text = cartoonChapter.GroupId;
            }

            if (!thumbnailHandle.IsDone)
            {
                yield return thumbnailHandle;
            }

            thumbnail.sprite = thumbnailHandle.Result;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Select();
        }

        private void Select()
        {
            if (currentSelected != null)
            {
                currentSelected.Deselect();
            }

            currentSelected = this;

            OnSelection(true);
        }

        private void Deselect()
        {
            if (currentSelected == this)
            {
                currentSelected = null;
            }

            OnSelection(false);
        }

        private void OnSelection(bool selected)
        {
            if (selectionCoroutine != null)
            {
                StopCoroutine(selectionCoroutine);
                selectionSequence.Kill();
                selectionCoroutine = null;
                selectionSequence = null;
            }

            selectionCoroutine = StartCoroutine(SelectionCoroutine(selected));
        }

        private IEnumerator SelectionCoroutine(bool selected)
        {
            float targetSelectionFrameAlpha = selected ? 1 : 0;
            float targetColorAdjustmentSaturation = selected ? 0 : -0.5f;

            var currentSelectionFrameAlpha = selectionFrame.color.a;
            var currentColorAdjustmentSaturation = colorAdjustmentOverlay.Saturation;

            selectionSequence = DOTween.Sequence();
            selectionSequence.Append(DOVirtual.Float(0, 1, 0.1f, value =>
            {
                var newColor = selectionFrame.color;
                newColor.a = Mathf.Lerp(currentSelectionFrameAlpha, targetSelectionFrameAlpha, value);
                selectionFrame.color = newColor;
                colorAdjustmentOverlay.Saturation = Mathf.Lerp(currentColorAdjustmentSaturation, targetColorAdjustmentSaturation, value);
            }));

            yield return selectionSequence.WaitForCompletion();

            selectionCoroutine = null;
            selectionSequence = null;
        }
    }
}