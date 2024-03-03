using System;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2.Scenario.UI
{
    public class UIManager : Singleton<UIManager>
    {
        #region Serialized

        public GameObject emptyUIObjectPrefab;
        public GameObject backgroundPrefab;

        public Canvas uiCanvas;

        public Canvas loadingCanvas;
        public FadeTransition fade;

        [SerializeField]
        private Canvas gameUICanvas;

        public Canvas contentCanvas;

        [SerializeField]
        private MultiStyleObject captionBoxPrefabs;

        [SerializeField]
        private MultiStyleObject messageBoxPrefabs;

        public Canvas backgroundCanvas;
        public Canvas live2DCanvas;
        public Canvas stillCanvas;
        public Canvas spriteCanvas;

        #endregion

        #region Non-serialized

        public CaptionBox CaptionBox { get; private set; }
        public MessageBox MessageBox { get; private set; }

        [NonSerialized]
        public Color CaptionDefaultColor;

        private RectTransform live2DCanvasRectTransform;
        private RectTransform backgroundCanvasRectTransform;

        #endregion

        protected override void Awake()
        {
            base.Awake();

            var captionBoxObj = captionBoxPrefabs.Instantiate(GameSettings.ConsoleStyle, gameUICanvas.transform);
            CaptionBox = captionBoxObj.GetComponent<CaptionBox>();
            captionBoxObj.SetActive(false);

            var messageBoxObj = messageBoxPrefabs.Instantiate(GameSettings.ConsoleStyle, gameUICanvas.transform);
            MessageBox = messageBoxObj.GetComponent<MessageBox>();
            messageBoxObj.SetActive(false);

            CaptionDefaultColor = Color.white;

            live2DCanvasRectTransform = live2DCanvas.GetComponent<RectTransform>();
            backgroundCanvasRectTransform = backgroundCanvas.GetComponent<RectTransform>();

            CameraPos = CameraPosDefault;
            CameraScale = CameraScaleDefault;

            // on awake, there are some things that need to be disabled
            uiCanvas.gameObject.SetActive(false);
            contentCanvas.gameObject.SetActive(false);
        }

        public static Vector2 CameraPos
        {
            get => Instance.live2DCanvasRectTransform.anchoredPosition * -1;
            set
            {
                Instance.live2DCanvasRectTransform.anchoredPosition = value * -1.0f;
                Instance.backgroundCanvasRectTransform.anchoredPosition = value * -1.0f;
            }
        }

        public static Vector2 CameraPosDefault => Vector2.zero;

        public static Vector2 CameraScale
        {
            get => Instance.live2DCanvasRectTransform.localScale;
            set
            {
                Instance.live2DCanvasRectTransform.localScale = new Vector3(value.x, value.y, Instance.live2DCanvasRectTransform.localScale.z);
                Instance.backgroundCanvasRectTransform.localScale = new Vector3(value.x, value.y, Instance.backgroundCanvasRectTransform.localScale.z);
            }
        }

        public static Vector2 CameraScaleDefault => Vector2.one;

        // public static Vector2 MessageBoxPos
        // {
        //     get => Instance.messageBox.rectTransform.anchoredPosition * -1;
        //     set => Instance.messageBox.rectTransform.anchoredPosition = value * -1.0f;
        // }

        [Serializable]
        private class MultiStyleObject
        {
            public GameObject mobileStyle;
            public GameObject consoleStyle;

            public GameObject Instantiate(bool console, Transform parent)
            {
                return UnityEngine.Object.Instantiate(console ? consoleStyle : mobileStyle, parent);
            }
        }
    }
}
