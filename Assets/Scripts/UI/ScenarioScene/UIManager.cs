using System;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2.UI.ScenarioScene
{
    public class UIManager : Singleton<UIManager>
    {
        public Canvas loadingCanvas;
        public FadeTransition fade;

        [Header("Caption")]
        public Canvas captionCanvas;
        public MultiStyleObject captionBoxPrefabs;

        public CaptionBox CaptionBox { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            var captionBoxObj = Instantiate(GameSettings.ConsoleStyle ? captionBoxPrefabs.consoleStyle : captionBoxPrefabs.mobileStyle, captionCanvas.transform);
            CaptionBox = captionBoxObj.GetComponent<CaptionBox>();
            captionBoxObj.SetActive(false);
        }

        [Serializable]
        public class MultiStyleObject
        {
            public GameObject mobileStyle;
            public GameObject consoleStyle;
        }
    }
}
