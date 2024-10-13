using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Kaede2
{
    public class CartoonChapterSelection : MonoBehaviour, IThemeChangeObserver, IPointerEnterHandler, IPointerClickHandler
    {
        [SerializeField]
        private CartoonChapterPanel panel;

        [SerializeField]
        private Image selectionFrame;

        [SerializeField]
        [Range(0, 1)]
        private float deselectedBrightness = 0.5f;

        private CartoonSceneController sceneController;

        private AsyncOperationHandle<Sprite> thumbnailHandle;
        private MasterCartoonInfo.CartoonInfo cartoonChapter;

        private Coroutine selectionCoroutine;
        private Sequence selectionSequence;

        public UnityEvent onSelect;

        private static CartoonChapterSelection currentSelected = null;
        public static CartoonChapterSelection CurrentSelected => currentSelected;

        // it seems that we can't rely on TMP's line wrapping
        private static readonly List<int> splitLineIndices = new()
        {
            7, 6, 6, 5, 8, 4, 4, -1, 8, -1, 5, 4, -1, -1, 5, -1, 5, 4
        };

        public CartoonChapterPanel Panel => panel;
        public MasterCartoonInfo.CartoonInfo ChapterInfo => cartoonChapter;

        private void Awake()
        {
            OnThemeChange(Theme.Current);

            if (currentSelected == null) Select();
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

        public IEnumerator Initialize(CartoonSceneController controller, int cartoonChapterNumber) // note: starts from 1
        {
            sceneController = controller;

            var newColor = selectionFrame.color;
            newColor.a = 0;
            selectionFrame.color = newColor;

            cartoonChapter = MasterCartoonInfo.Instance.Data
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

            panel.Title = groupTitle;
            panel.ChapterNumber = cartoonChapter.GroupId;

            if (!thumbnailHandle.IsDone)
            {
                yield return thumbnailHandle;
            }

            panel.Thumbnail = thumbnailHandle.Result;
        }

        public void Select()
        {
            if (currentSelected != null)
            {
                currentSelected.Deselect();
            }

            currentSelected = this;

            onSelect.Invoke();
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
            var currentBrightness = panel.Brightness;
            float targetBrightness = selected ? 1 : deselectedBrightness;

            var currentSelectionFrameAlpha = selectionFrame.color.a;
            float targetSelectionFrameAlpha = selected ? 1 : 0;

            selectionSequence = DOTween.Sequence();
            selectionSequence.Append(DOVirtual.Float(0, 1, 0.1f, value =>
            {
                var newColor = selectionFrame.color;
                newColor.a = Mathf.Lerp(currentSelectionFrameAlpha, targetSelectionFrameAlpha, value);
                selectionFrame.color = newColor;

                panel.Brightness = Mathf.Lerp(currentBrightness, targetBrightness, value);
            }));

            yield return selectionSequence.WaitForCompletion();

            selectionCoroutine = null;
            selectionSequence = null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Select();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            sceneController.OnChapterSelected(this);
        }
    }
}