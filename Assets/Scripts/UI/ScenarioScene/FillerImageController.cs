using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaede2.UI.ScenarioScene
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

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
        }

        private void Update()
        {
            float aspectRatio = 16.0f / 9.0f;

            bool playMode = !Application.isEditor || Application.isPlaying;
            bool shouldFix16By9 = playMode && 
                                  GameSettings.Fixed16By9 && Screen.width * 9.0f > Screen.height * 16.0f;
            if (!playMode || !shouldFix16By9)
                aspectRatio = (float) Screen.width / Screen.height;

            float canvasWidth = aspectRatio * rt.rect.height;

            imageLeft.anchoredPosition = new Vector2(-canvasWidth / 2, imageLeft.anchoredPosition.y);
            imageRight.anchoredPosition = new Vector2(canvasWidth / 2, imageRight.anchoredPosition.y);
        }
    }
}
