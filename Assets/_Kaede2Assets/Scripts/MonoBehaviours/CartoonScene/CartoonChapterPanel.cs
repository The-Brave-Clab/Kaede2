using Kaede2.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class CartoonChapterPanel : MonoBehaviour
    {
        [SerializeField]
        private Image thumbnail;

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
        private float brightness = 1.0f;

        private string title;
        private string chapterNumber;

        private VertexGradient titleGradient;
        private VertexGradient circleGradient;

        public float Brightness
        {
            get => brightness;
            set
            {
                brightness = value;
                UpdateBrightness();
            }
        }

        public string Title
        {
            get => title;
            set
            {
                title = value;
                titleText.text = title;
                titleOutlineText.text = title;
                titleBackgroundText.text = title;
                titleBackgroundOutlineText.text = title;
                titleBackgroundShadowText.text = title;
            }
        }

        public string ChapterNumber
        {
            get => chapterNumber;
            set
            {
                chapterNumber = value;
                chapterNumberText.text = chapterNumber;
                chapterNumberOutlineText.text = chapterNumber;
                chapterNumberShadowText.text = chapterNumber;
            }
        }

        public Sprite Thumbnail
        {
            get => thumbnail.sprite;
            set => thumbnail.sprite = value;
        }

        private void Awake()
        {
            titleGradient = new VertexGradient(titleGradientTop, titleGradientTop, titleGradientBottom, titleGradientBottom);
            circleGradient = new VertexGradient(circleGradientTop, circleGradientTop, circleGradientBottom, circleGradientBottom);

            UpdateBrightness();
        }

        private void UpdateBrightness()
        {
            titleText.colorGradient = titleGradient.Multiply(brightness).NoAlpha();
            titleOutlineText.color = (titleOutline * brightness).NoAlpha();
            titleBackgroundText.color = (titleBackground * brightness).NoAlpha();
            titleBackgroundOutlineText.color = (titleBackgroundOutline * brightness).NoAlpha();
            titleBackgroundShadowText.color = (titleBackgroundOutline * brightness).NoAlpha();
            var circleGradientDeselected = circleGradient.Multiply(brightness).NoAlpha();
            circleText.color = (circleColor * brightness).NoAlpha();
            circleOutlineText.colorGradient = circleGradientDeselected;
            chapterNumberText.colorGradient = circleGradientDeselected;
            chapterNumberOutlineText.color = (circleTextOutline * brightness).NoAlpha();
            chapterNumberShadowText.color = (circleTextShadow * brightness).NoAlpha();

            var deselectedColor = new Color(brightness, brightness, brightness, 1);
            thumbnail.color = deselectedColor;
            logo.color = deselectedColor;
            background.color = deselectedColor;
        }
    }
}