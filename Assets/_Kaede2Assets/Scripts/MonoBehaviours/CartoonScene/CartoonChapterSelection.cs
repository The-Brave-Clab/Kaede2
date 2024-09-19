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
        private Image background;

        [SerializeField]
        private Image logo;

        [SerializeField]
        private TextMeshProUGUI titleText;

        [SerializeField]
        private TextMeshProUGUI titleOutlineText;

        [SerializeField]
        private TextMeshProUGUI titleBackgroundText;

        [SerializeField]
        private TextMeshProUGUI titleBackgroundOutlineText;

        [SerializeField]
        private TextMeshProUGUI titleBackgroundShadowText;

        [SerializeField]
        private TextMeshProUGUI chapterNumberText;

        [SerializeField]
        private TextMeshProUGUI chapterNumberOutlineText;

        [SerializeField]
        private TextMeshProUGUI chapterNumberShadowText;

        [SerializeField]
        private TextMeshProUGUI circleText;

        [SerializeField]
        private TextMeshProUGUI circleOutlineText;

        [SerializeField]
        private int index;

        [SerializeField]
        private Color titleGradientTop;

        [SerializeField]
        private Color titleGradientBottom;

        [SerializeField]
        private Color titleOutline;

        [SerializeField]
        private Color titleBackground;

        [SerializeField]
        private Color titleBackgroundOutline;

        [SerializeField]
        private Color circleGradientTop;

        [SerializeField]
        private Color circleGradientBottom;

        [SerializeField]
        private Color circleColor;

        [SerializeField]
        private Color circleTextOutline;

        [SerializeField]
        private Color circleTextShadow;

        [SerializeField]
        [Range(0, 1)]
        private float deselectedBrightness = 0.5f;

        private AsyncOperationHandle<Sprite> thumbnailHandle;
        private MasterCartoonInfo.CartoonInfo cartoonChapter;

        private VertexGradient titleGradient;
        private VertexGradient circleGradient;

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

            titleGradient = new VertexGradient(titleGradientTop, titleGradientTop, titleGradientBottom, titleGradientBottom);
            circleGradient = new VertexGradient(circleGradientTop, circleGradientTop, circleGradientBottom, circleGradientBottom);
        }

        private IEnumerator Start()
        {
            // start with deselected state
            titleText.colorGradient = titleGradient.Multiply(deselectedBrightness).NoAlpha();
            titleOutlineText.color = (titleOutline * deselectedBrightness).NoAlpha();
            titleBackgroundText.color = (titleBackground * deselectedBrightness).NoAlpha();
            titleBackgroundOutlineText.color = (titleBackgroundOutline * deselectedBrightness).NoAlpha();
            titleBackgroundShadowText.color = (titleBackgroundOutline * deselectedBrightness).NoAlpha();
            var circleGradientDeselected = circleGradient.Multiply(deselectedBrightness).NoAlpha();
            circleText.color = (circleColor * deselectedBrightness).NoAlpha();
            circleOutlineText.colorGradient = circleGradientDeselected;
            chapterNumberText.colorGradient = circleGradientDeselected;
            chapterNumberOutlineText.color = (circleTextOutline * deselectedBrightness).NoAlpha();
            chapterNumberShadowText.color = (circleTextShadow * deselectedBrightness).NoAlpha();

            var deselectedColor = new Color(deselectedBrightness, deselectedBrightness, deselectedBrightness, 1);
            thumbnail.color = deselectedColor;
            logo.color = deselectedColor;
            background.color = deselectedColor;

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

            titleText.text = groupTitle;
            titleOutlineText.text = groupTitle;
            titleBackgroundText.text = groupTitle;
            titleBackgroundOutlineText.text = groupTitle;
            titleBackgroundShadowText.text = groupTitle;

            chapterNumberText.text = cartoonChapter.GroupId;
            chapterNumberOutlineText.text = cartoonChapter.GroupId;
            chapterNumberShadowText.text = cartoonChapter.GroupId;

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
            float targetBrightness = selected ? 1 : deselectedBrightness;

            var currentSelectionFrameAlpha = selectionFrame.color.a;
            float targetSelectionFrameAlpha = selected ? 1 : 0;

            var currentTitleGradient = titleText.colorGradient;
            var targetTitleGradient = titleGradient.Multiply(targetBrightness);

            var currentTitleOutline = titleOutlineText.color;
            var targetTitleOutline = titleOutline * targetBrightness;

            var currentTitleBackground = titleBackgroundText.color;
            var targetTitleBackground = titleBackground * targetBrightness;

            var currentTitleBackgroundOutline = titleBackgroundOutlineText.color;
            var targetTitleBackgroundOutline = titleBackgroundOutline * targetBrightness;

            var currentCircle = circleText.color;
            var targetCircle = circleColor * targetBrightness;

            var currentCircleOutlineGradient = circleOutlineText.colorGradient;
            var targetCircleGradient = circleGradient.Multiply(targetBrightness);

            var currentChapterNumberGradient = chapterNumberText.colorGradient;
            var targetChapterNumberGradient = targetCircleGradient;

            var currentChapterNumberOutline = chapterNumberOutlineText.color;
            var targetChapterNumberOutline = circleTextOutline * targetBrightness;

            var currentChapterNumberShadow = chapterNumberShadowText.color;
            var targetChapterNumberShadow = circleTextShadow * targetBrightness;

            var currentImageComponentColor = thumbnail.color;
            var targetImageComponentColor = new Color(targetBrightness, targetBrightness, targetBrightness, 1);

            selectionSequence = DOTween.Sequence();
            selectionSequence.Append(DOVirtual.Float(0, 1, 0.1f, value =>
            {
                var newColor = selectionFrame.color;
                newColor.a = Mathf.Lerp(currentSelectionFrameAlpha, targetSelectionFrameAlpha, value);
                selectionFrame.color = newColor;

                titleText.colorGradient = CommonUtils.LerpVertexGradient(currentTitleGradient, targetTitleGradient, value).NoAlpha();
                titleOutlineText.color = Color.Lerp(currentTitleOutline, targetTitleOutline, value).NoAlpha();
                titleBackgroundText.color = Color.Lerp(currentTitleBackground, targetTitleBackground, value).NoAlpha();
                titleBackgroundOutlineText.color = Color.Lerp(currentTitleBackgroundOutline, targetTitleBackgroundOutline, value).NoAlpha();
                titleBackgroundShadowText.color = Color.Lerp(currentTitleBackgroundOutline, targetTitleBackgroundOutline, value).NoAlpha();
                circleText.color = Color.Lerp(currentCircle, targetCircle, value).NoAlpha();
                circleOutlineText.colorGradient = CommonUtils.LerpVertexGradient(currentCircleOutlineGradient, targetCircleGradient, value).NoAlpha();
                chapterNumberText.colorGradient = CommonUtils.LerpVertexGradient(currentChapterNumberGradient, targetChapterNumberGradient, value).NoAlpha();
                chapterNumberOutlineText.color = Color.Lerp(currentChapterNumberOutline, targetChapterNumberOutline, value).NoAlpha();
                chapterNumberShadowText.color = Color.Lerp(currentChapterNumberShadow, targetChapterNumberShadow, value).NoAlpha();

                thumbnail.color = Color.Lerp(currentImageComponentColor, targetImageComponentColor, value).NoAlpha();
                logo.color = thumbnail.color;
                background.color = thumbnail.color;
            }));

            yield return selectionSequence.WaitForCompletion();

            selectionCoroutine = null;
            selectionSequence = null;
        }
    }
}