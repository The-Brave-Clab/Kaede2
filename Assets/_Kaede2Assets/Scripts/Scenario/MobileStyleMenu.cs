using System;
using Kaede2.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.Scenario
{
    public class MobileStyleMenu : MonoBehaviour
    {
        [SerializeField]
        private PlayerUIController uiController;

        [SerializeField]
        private Button showMenuButton;

        [SerializeField]
        private GameObject menuHolder;

        [Header("Buttons")]
        [SerializeField]
        private Button webExitFullscreenButton;

        [SerializeField]
        private Button hideUIButton;

        [SerializeField]
        private Button logButton;

        [SerializeField]
        private Button autoButton;

        [SerializeField]
        private Button continuousButton;

        [SerializeField]
        private Button goBackButton;

        private bool hidden;
        private bool expanded;

        private void Awake()
        {
            void Shrink()
            {
                expanded = false;
                UpdateMobileStyleMenuVisibility();
            }

            void Expand()
            {
                expanded = true;
                UpdateMobileStyleMenuVisibility();
            }

            showMenuButton.onClick.AddListener(Expand);
    
            webExitFullscreenButton.onClick.AddListener(Shrink);
            hideUIButton.onClick.AddListener(Shrink);
            logButton.onClick.AddListener(Shrink);
            autoButton.onClick.AddListener(Shrink);
            continuousButton.onClick.AddListener(Shrink);
            goBackButton.onClick.AddListener(Shrink);

#if UNITY_WEBGL && !UNITY_EDITOR
            webExitFullscreenButton.gameObject.SetActive(true);
            logButton.gameObject.SetActive(false);
            goBackButton.gameObject.SetActive(false);

            webExitFullscreenButton.onClick.AddListener(() =>
            {
                WebInterop.OnExitFullscreen();
                WebInterop.Instance.ChangeFullscreen(0);
            });
#else
            webExitFullscreenButton.gameObject.SetActive(false);
            logButton.gameObject.SetActive(true);
            goBackButton.gameObject.SetActive(true);
#endif

            hideUIButton.gameObject.SetActive(true);
            autoButton.gameObject.SetActive(true);
            continuousButton.gameObject.SetActive(true);

            expanded = false;

            UpdateMobileStyleMenuVisibility();

            InputManager.onDeviceTypeChanged += OnDeviceTypeChanged;
        }

        private void OnDestroy()
        {
            InputManager.onDeviceTypeChanged -= OnDeviceTypeChanged;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private bool lastFullscreen = false;

        private void Update()
        {
            if (lastFullscreen != WebInterop.Fullscreen)
            {
                lastFullscreen = WebInterop.Fullscreen;
                UpdateMobileStyleMenuVisibility();
            }
        }
#endif

        private void OnDeviceTypeChanged(InputDeviceType type)
        {
            UpdateMobileStyleMenuVisibility();
        }

        private void UpdateMobileStyleMenuVisibility()
        {
            // in web build, show the menu only when in fullscreen
            // otherwise, show the menu only when using touchscreen
#if UNITY_WEBGL && !UNITY_EDITOR
            hidden = !WebInterop.Fullscreen;
#else
            hidden = InputManager.CurrentDeviceType != InputDeviceType.Touchscreen;
#endif
            showMenuButton.gameObject.SetActive(!expanded && !hidden);
            menuHolder.SetActive(expanded && !hidden);
        }
    }
}