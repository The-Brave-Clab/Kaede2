using UnityEngine;

namespace Kaede2.Scenario.Framework.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class FillerImageController : MonoBehaviour
    {
        [SerializeField]
        private RectTransform imageLeft;

        [SerializeField]
        private RectTransform imageRight;

        private RectTransform rt;

        public UIController UIController { get; set; }

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
        }

        private void Update()
        {
            float aspectRatio = 16.0f / 9.0f;

            bool playMode = !Application.isEditor || Application.isPlaying;
            bool shouldFix16By9 = playMode && 
                                  UIController.Module.Fixed16By9 && Screen.width * 9.0f > Screen.height * 16.0f;
            if (!playMode || !shouldFix16By9)
                aspectRatio = (float) Screen.width / Screen.height;

            float canvasWidth = aspectRatio * rt.rect.height;

            imageLeft.anchoredPosition = new Vector2(-canvasWidth / 2, imageLeft.anchoredPosition.y);
            imageRight.anchoredPosition = new Vector2(canvasWidth / 2, imageRight.anchoredPosition.y);
        }
    }
}
