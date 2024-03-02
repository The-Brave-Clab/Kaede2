using System;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2.UI.ScenarioScene
{
    public class UIManager : Singleton<UIManager>
    {
        #region Serialized

        public Canvas loadingCanvas;
        public FadeTransition fade;

        [SerializeField]
        private Canvas gameUICanvas;

        [SerializeField]
        private MultiStyleObject captionBoxPrefabs;

        [SerializeField]
        private MultiStyleObject messageBoxPrefabs;

        #endregion

        #region Non-serialized

        public CaptionBox CaptionBox { get; private set; }
        public MessageBox MessageBox { get; private set; }

        [NonSerialized]
        public Color CaptionDefaultColor;

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
        }

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
