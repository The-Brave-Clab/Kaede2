using System;
using Kaede2.Input;
using Kaede2.Scenario.Framework;
using Kaede2.Scenario.Framework.UI;
using Kaede2.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kaede2.Scenario
{
    public class PlayerUIController : UIController
    {
        public override Canvas UICanvas => uiCanvas;
        public override Canvas ContentCanvas => contentCanvas;
        public override Canvas LoadingCanvas => loadingCanvas;
        public override FillerImageController Filler => filler;

        public override Canvas BackgroundCanvas => backgroundCanvas;
        public override Canvas Live2DCanvas => live2DCanvas;
        public override Canvas StillCanvas => stillCanvas;
        public override Canvas SpriteCanvas => spriteCanvas;

        public override CaptionBox CaptionBox => instantiatedCaptionBox;
        public override MessageBox MessageBox => instantiatedMessageBox;

        public override FadeTransition Fade => fade;

        public ButtonForPointer MesButton => mesButton;
        public MobileStyleMenu MobileStyleMenu => mobileStyleMenu;

        [SerializeField]
        private Canvas uiCanvas;
        [SerializeField]
        private Canvas contentCanvas;
        [SerializeField]
        private Canvas loadingCanvas;
        [SerializeField]
        private FillerImageController filler;

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

        [SerializeField]
        private ButtonForPointer mesButton;

        [SerializeField]
        private MobileStyleMenu mobileStyleMenu;
        [SerializeField]
        private GameObject exitFullscreenButton;

        [SerializeField]
        private LogPanel logPanel;

        private CaptionBox instantiatedCaptionBox;
        private MessageBox instantiatedMessageBox;

        private bool uiHidden;

        public bool UIHidden
        {
            get => uiHidden;
            set
            {
                uiHidden = value;
                UpdateUIVisibility();
            }
        }

        protected override void Awake()
        {
            var captionBoxObj = captionBoxPrefabs.Instantiate(GameSettings.ConsoleStyle, gameUICanvas.transform);
            instantiatedCaptionBox = captionBoxObj.GetComponent<CaptionBox>();

            var messageBoxObj = messageBoxPrefabs.Instantiate(GameSettings.ConsoleStyle, gameUICanvas.transform);
            instantiatedMessageBox = messageBoxObj.GetComponent<MessageBox>();
            instantiatedMessageBox.DisableAutoModeAction = () => Module.AutoMode = false;
            instantiatedMessageBox.DisableContinuousModeAction = () => Module.ContinuousMode = false;

            uiHidden = false;

            base.Awake();
        }

        private void OnEnable()
        {
            InputManager.onDeviceTypeChanged += OnDeviceTypeChanged;
            InputManager.InputAction.Scenario.ToggleUI.performed += OnToggleUI;
#if !UNITY_WEBGL || UNITY_EDITOR
            // don't allow log panel in web build
            InputManager.InputAction.Scenario.ShowLog.performed += ShowLogPanel;
#endif
        }

        private void OnDisable()
        {
            InputManager.onDeviceTypeChanged -= OnDeviceTypeChanged;
            if (InputManager.InputAction != null)
            {
                InputManager.InputAction.Scenario.ToggleUI.performed -= OnToggleUI;
#if !UNITY_WEBGL || UNITY_EDITOR
                InputManager.InputAction.Scenario.ShowLog.performed -= ShowLogPanel;
#endif
            }
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

        public void ToggleUI()
        {
            uiHidden = !uiHidden;
            UpdateUIVisibility();
        }

        private void OnDeviceTypeChanged(InputDeviceType type)
        {
            UpdateUIVisibility();
        }

        private void OnToggleUI(InputAction.CallbackContext ctx)
        {
            ToggleUI();
        }

        private void UpdateUIVisibility()
        {
            instantiatedMessageBox.Hidden = uiHidden;
        }

        private void ShowLogPanel(InputAction.CallbackContext ctx)
        {
            logPanel.Enable(true);
        }
    }
}
