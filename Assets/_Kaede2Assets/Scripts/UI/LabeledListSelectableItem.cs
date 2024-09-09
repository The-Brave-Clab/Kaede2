using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Kaede2.ScriptableObjects;
using Kaede2.UI.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI
{
    public class LabeledListSelectableItem : SelectableItem, IThemeChangeObserver
    {
        [SerializeField]
        private TextMeshProUGUI mainLabel;

        [SerializeField]
        private TextMeshProUGUI mainText;

        [SerializeField]
        private RectTransform notSelectedParent;

        [SerializeField]
        private TextMeshProUGUI notSelectedLabel;

        [SerializeField]
        private TextMeshProUGUI notSelectedText;

        [SerializeField]
        private RectTransform selectedParent;

        [SerializeField]
        private TextMeshProUGUI selectedLabel;

        [SerializeField]
        private TextMeshProUGUI selectedText;

        [SerializeField]
        private Color notSelectedTextColor;

        [SerializeField]
        private RectTransform safeAreaContainer;

        private RectTransform rt;
        private Dictionary<TextMeshProUGUI, RectTransform> textRectTransforms;
        private VerticalLayoutGroup layoutGroup;
        private LayoutGroup selectedLayoutGroup;
        private LayoutGroup notSelectedLayoutGroup;

        private List<Material> mainTextMaterials;
        private List<Material> mainLabelMaterials;

        private Coroutine selectCoroutine;
        private Sequence selectSequence;

        private Color textRim;

        public string Label
        {
            get => mainLabel.text;
            set
            {
                mainLabel.text = value;
                notSelectedLabel.text = value;
                selectedLabel.text = value;
                ForceUpdate();
                UpdateMaterials();
            }
        }

        public string Text
        {
            get => mainText.text;
            set
            {
                mainText.text = value;
                notSelectedText.text = value;
                selectedText.text = value;
                ForceUpdate();
                UpdateMaterials();
            }
        }

        protected override void Awake()
        {
            OnThemeChange(Theme.Current);

            base.Awake();

            rt = GetComponent<RectTransform>();
            layoutGroup = rt.parent.GetComponent<VerticalLayoutGroup>();
            selectedLayoutGroup = selectedParent.GetComponent<LayoutGroup>();
            notSelectedLayoutGroup = notSelectedParent.GetComponent<LayoutGroup>();
            textRectTransforms = new Dictionary<TextMeshProUGUI, RectTransform>
            {
                { mainLabel, mainLabel.GetComponent<RectTransform>() },
                { mainText, mainText.GetComponent<RectTransform>() },
                { notSelectedLabel, notSelectedLabel.GetComponent<RectTransform>() },
                { notSelectedText, notSelectedText.GetComponent<RectTransform>() },
                { selectedLabel, selectedLabel.GetComponent<RectTransform>() },
                { selectedText, selectedText.GetComponent<RectTransform>() },
            };

            onSelected.AddListener(() => OnSelected(true));
            onDeselected.AddListener(() => OnSelected(false));

            lastSelectionState = false;
        }

        private IEnumerator Start()
        {
            // wait for layout to be calculated
            ForceUpdate();
            UpdateMaterials();
            yield return null;

            UpdateSafeArea();

            // set initial state
            RectTransform mainTextRectTransform = textRectTransforms[mainText];
            RectTransform mainLabelRectTransform = textRectTransforms[mainLabel];

            var layoutText = selected ? selectedText : notSelectedText;
            var layoutLabel = selected ? selectedLabel : notSelectedLabel;
            var targetParent = selected ? selectedParent : notSelectedParent;

            var targetTransform = GetLayoutObjectTransform(layoutText);
            var targetLabelTransform = GetLayoutObjectTransform(layoutLabel);
            var targetHeight = targetParent.rect.height;
            var targetTextColor = selected ? Color.white : notSelectedTextColor;
            var targetLabelFontSize = layoutLabel.fontSize;

            rt.sizeDelta = new Vector2(rt.sizeDelta.x, targetHeight);
            mainTextRectTransform.anchoredPosition = targetTransform.position;
            mainTextRectTransform.sizeDelta = targetTransform.size;
            mainLabelRectTransform.anchoredPosition = targetLabelTransform.position;
            mainLabelRectTransform.sizeDelta = targetLabelTransform.size;
            mainText.color = targetTextColor;
            mainLabel.color = targetTextColor;
            mainLabel.fontSize = targetLabelFontSize;

            var outlineColor = selected ? textRim : Color.black;
            var outlineWidth = selected ? 0.35f : 0;
            var faceDilate = selected ? 0.35f : 0;
            foreach (var material in mainTextMaterials)
            {
                material.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
                material.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
                material.SetFloat(ShaderUtilities.ID_FaceDilate, faceDilate);
            }
            foreach (var material in mainLabelMaterials)
            {
                material.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
                material.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
                material.SetFloat(ShaderUtilities.ID_FaceDilate, faceDilate);
            }

            ForceUpdate();
        }

        protected override void Update()
        {
            UpdateSafeArea();
            base.Update();
        }

        private bool lastSelectionState;
        private void OnSelected(bool isSelected)
        {
            bool needUpdate = false;
    
            if (selectCoroutine != null)
            {
                StopCoroutine(selectCoroutine);
                selectSequence.Kill();
                selectCoroutine = null;
                selectSequence = null;
                needUpdate = true;
            }

            if (lastSelectionState != isSelected)
            {
                needUpdate = true;
                lastSelectionState = isSelected;
            }

            if (needUpdate)
            {
                selectCoroutine = StartCoroutine(SelectCoroutine(isSelected));
            }
        }

        private IEnumerator SelectCoroutine(bool isSelected)
        {
            UpdateSafeArea();

            RectTransform mainTextRectTransform = textRectTransforms[mainText];
            RectTransform mainLabelRectTransform = textRectTransforms[mainLabel];

            var layoutText = isSelected ? selectedText : notSelectedText;
            var layoutLabel = isSelected ? selectedLabel : notSelectedLabel;
            var targetParent = isSelected ? selectedParent : notSelectedParent;

            var currentHeight = rt.rect.height;
            var currentTextPosition = mainTextRectTransform.anchoredPosition;
            var currentTextSize = mainTextRectTransform.sizeDelta;
            var currentLabelPosition = mainLabelRectTransform.anchoredPosition;
            var currentLabelSize = mainLabelRectTransform.sizeDelta;
            var currentTextColor = mainText.color;
            var currentLabelFontSize = mainLabel.fontSize;
            var currentOutlineColor = mainText.fontMaterial.GetColor(ShaderUtilities.ID_OutlineColor);
            var currentOutlineWidth = mainText.fontMaterial.GetFloat(ShaderUtilities.ID_OutlineWidth);

            var targetHeight = targetParent.rect.height;
            var targetTextColor = isSelected ? Color.white : notSelectedTextColor;
            var targetLabelFontSize = layoutLabel.fontSize;
            var targetOutlineColor = isSelected ? textRim : Color.black;
            var targetOutlineWidth = isSelected ? 0.35f : 0;

            selectSequence = DOTween.Sequence();
            selectSequence.Append(DOVirtual.Float(0, 1, 0.1f, value =>
            {
                // we do this every frame since it might move during the animation
                var targetTransform = GetLayoutObjectTransform(layoutText);
                var targetLabelTransform = GetLayoutObjectTransform(layoutLabel);

                rt.sizeDelta = new Vector2(rt.sizeDelta.x, Mathf.Lerp(currentHeight, targetHeight, value));
                mainTextRectTransform.anchoredPosition = Vector2.Lerp(currentTextPosition, targetTransform.position, value);
                mainTextRectTransform.sizeDelta = Vector2.Lerp(currentTextSize, targetTransform.size, value);
                mainLabelRectTransform.anchoredPosition = Vector2.Lerp(currentLabelPosition, targetLabelTransform.position, value);
                mainLabelRectTransform.sizeDelta = Vector2.Lerp(currentLabelSize, targetLabelTransform.size, value);
                mainText.color = Color.Lerp(currentTextColor, targetTextColor, value);
                mainLabel.color = Color.Lerp(currentTextColor, targetTextColor, value);
                mainLabel.fontSize = Mathf.RoundToInt(Mathf.Lerp(currentLabelFontSize, targetLabelFontSize, value));

                var outlineColor = Color.Lerp(currentOutlineColor, targetOutlineColor, value);
                var outlineWidth = Mathf.Lerp(currentOutlineWidth, targetOutlineWidth, value);
                var faceDilate = Mathf.Lerp(currentOutlineWidth, targetOutlineWidth, value);
                foreach (var material in mainTextMaterials)
                {
                    material.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
                    material.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
                    material.SetFloat(ShaderUtilities.ID_FaceDilate, faceDilate);
                }
                foreach (var material in mainLabelMaterials)
                {
                    material.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
                    material.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
                    material.SetFloat(ShaderUtilities.ID_FaceDilate, faceDilate);
                }

                ForceUpdate();
            }));

            yield return selectSequence.WaitForCompletion();

            selectCoroutine = null;
            selectSequence = null;
        }

        private (Vector2 position, Vector2 size) GetLayoutObjectTransform(TextMeshProUGUI layoutObject)
        {
            RectTransform lrt = textRectTransforms[layoutObject];
            var worldCorners = new Vector3[4];
            lrt.GetWorldCorners(worldCorners);

            // transform world corners to local space
            var localCorners = new Vector3[4];
            for (var i = 0; i < 4; i++)
            {
                localCorners[i] = safeAreaContainer.InverseTransformPoint(worldCorners[i]);
            }

            // calculate size and position
            var size = new Vector2(localCorners[2].x - localCorners[0].x, localCorners[2].y - localCorners[0].y);
            var position = new Vector2(localCorners[0].x + size.x * lrt.pivot.x, localCorners[0].y + size.y * lrt.pivot.y);

            return (position, size);
        }

        private void ForceUpdate()
        {
            // force layout update
            layoutGroup.SetLayoutVertical();
            selectedLayoutGroup.SetLayoutHorizontal();
            notSelectedLayoutGroup.SetLayoutHorizontal();
            // force text update
            mainText.UpdateFontAsset();
            mainLabel.UpdateFontAsset();
        }

        private void UpdateMaterials()
        {
            mainTextMaterials ??= new List<Material>();
            mainLabelMaterials ??= new List<Material>();

            mainTextMaterials.Clear();
            mainLabelMaterials.Clear();

            mainTextMaterials.Add(mainText.fontMaterial);
            mainTextMaterials.Add(mainText.materialForRendering);
            var mainTextSubMeshes = mainText.GetComponentsInChildren<TMP_SubMeshUI>();
            foreach (var subMesh in mainTextSubMeshes)
            {
                mainTextMaterials.Add(subMesh.materialForRendering);
            }

            mainLabelMaterials.Add(mainLabel.fontMaterial);
            mainLabelMaterials.Add(mainLabel.materialForRendering);
            var mainLabelSubMeshes = mainLabel.GetComponentsInChildren<TMP_SubMeshUI>();
            foreach (var subMesh in mainLabelSubMeshes)
            {
                mainLabelMaterials.Add(subMesh.materialForRendering);
            }
        }

        private void UpdateSafeArea()
        {
            float additionalX = Screen.orientation == ScreenOrientation.LandscapeLeft
                ? Screen.safeArea.x / safeAreaContainer.lossyScale.x
                : 0; // i'll be damned if any device have safe area on the bottom side of the screen
            safeAreaContainer.offsetMin = new Vector2(additionalX, safeAreaContainer.offsetMin.y);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            textRim = theme.MainTextRim;
        }
    }

}
