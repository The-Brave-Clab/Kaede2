using System;
using Kaede2.Scenario.Base;
using UnityEngine;

namespace Kaede2.Scenario.UI
{
    public class UIController : UIControllerBase
    {
        public override Canvas UICanvas => uiCanvas;
        public override Canvas ContentCanvas => contentCanvas;
        public override Canvas LoadingCanvas => loadingCanvas;

        public override Canvas BackgroundCanvas => backgroundCanvas;
        public override Canvas Live2DCanvas => live2DCanvas;
        public override Canvas StillCanvas => stillCanvas;
        public override Canvas SpriteCanvas => spriteCanvas;

        public override CaptionBox CaptionBox => instantiatedCaptionBox;
        public override MessageBox MessageBox => instantiatedMessageBox;

        public override FadeTransition Fade => fade;

        [SerializeField]
        private Canvas uiCanvas;
        [SerializeField]
        private Canvas contentCanvas;
        [SerializeField]
        private Canvas loadingCanvas;

        [SerializeField]
        private Canvas backgroundCanvas;
        [SerializeField]
        private Canvas live2DCanvas;
        [SerializeField]
        private Canvas stillCanvas;
        [SerializeField]
        private Canvas spriteCanvas;

        [SerializeField]
        private MultiStyleObject captionBoxPrefabs;

        [SerializeField]
        private MultiStyleObject messageBoxPrefabs;

        [SerializeField]
        private FadeTransition fade;

        [SerializeField]
        private Canvas gameUICanvas;

        private CaptionBox instantiatedCaptionBox;
        private MessageBox instantiatedMessageBox;

        protected override void Awake()
        {
            var captionBoxObj = captionBoxPrefabs.Instantiate(GameSettings.ConsoleStyle, gameUICanvas.transform);
            instantiatedCaptionBox = captionBoxObj.GetComponent<CaptionBox>();

            var messageBoxObj = messageBoxPrefabs.Instantiate(GameSettings.ConsoleStyle, gameUICanvas.transform);
            instantiatedMessageBox = messageBoxObj.GetComponent<MessageBox>();

            base.Awake();
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
