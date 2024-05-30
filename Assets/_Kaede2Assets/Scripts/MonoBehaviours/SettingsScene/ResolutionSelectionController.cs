using System.Collections.Generic;
using System.Linq;
using Kaede2.Localization;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.Localization.Components;
using LocalizeStringEvent = UnityEngine.Localization.Components.LocalizeStringEvent;

namespace Kaede2
{
    public class ResolutionSelectionController : MonoBehaviour
    {
        [SerializeField]
        private SelectionControl selectionControl;

        [SerializeField]
        private SelectionControl fullscreenControl;

        [SerializeField]
        private CommonButton applyButton;

        [SerializeField]
        private GameObject confirmBoxPrefab;

        [SerializeField]
        private Canvas mainCanvas;

        // only contains 16:9 and 21:9 resolutions
        private static readonly List<Vector2Int> commonWindowModeResolutions = new()
        {
            new(1280, 720), // 16:9
            new(1366, 768), // 16:9
            new(1440, 900), // 16:10
            new(1600, 900), // 16:9
            new(1680, 1050), // 16:10
            new(1920, 1080), // 16:9
            new(1920, 1200), // 16:10
            new(2560, 1080), // 21:9
            new(2560, 1440), // 16:9
            new(2560, 1600), // 16:10
            new(3840, 2160), // 16:9
            new(4096, 2160), // 17:9
            new(5120, 2160), // 21:9
            new(5120, 2880), // 16:9
            new(5120, 3200), // 16:10
            new(7680, 4320), // 16:9
        };

        private List<Vector2Int> fullScreenResolutions;
        private List<Vector2Int> windowModeResolutions;

        Vector2Int currentResolution;
        bool currentFullscreen;

        private void Awake()
        {
            fullScreenResolutions = new();
            windowModeResolutions = new();

            Vector2Int maxResolution = new(Screen.currentResolution.width, Screen.currentResolution.height);
            foreach (var resolution in Screen.resolutions)
            {
                var vector2Int = new Vector2Int(resolution.width, resolution.height);
                if (!fullScreenResolutions.Contains(vector2Int))
                {
                    fullScreenResolutions.Add(vector2Int);
                }
                maxResolution = Vector2Int.Max(maxResolution, vector2Int);
            }

            foreach (var resolution in commonWindowModeResolutions
                         .Where(r => r.x <= maxResolution.x && r.y <= maxResolution.y))
            {
                windowModeResolutions.Add(resolution);
            }

            List<Vector2Int> resolutions = Screen.fullScreen ? fullScreenResolutions : windowModeResolutions;
            var bestResolutionIndex = RefreshItems(resolutions, Screen.fullScreen, new Vector2Int(Screen.width, Screen.height));
            selectionControl.SelectImmediate(bestResolutionIndex, false);

            currentResolution = resolutions[bestResolutionIndex];
            currentFullscreen = Screen.fullScreen;

            fullscreenControl.SelectImmediate(Screen.fullScreen ? 1 : 0, false);

            applyButton.Interactable = false;
        }

        private int RefreshItems(List<Vector2Int> resolutions, bool fullscreen, Vector2Int preferredResolution)
        {
            selectionControl.Clear();

            int bestResolutionIndex = -1;
            int bestResolutionDistance = int.MaxValue;

            for (var i = 0; i < resolutions.Count; i++)
            {
                var resolution = resolutions[i];
                int distance = Mathf.Abs(resolution.x - preferredResolution.x) + Mathf.Abs(resolution.y - preferredResolution.y);
                if (distance < bestResolutionDistance)
                {
                    bestResolutionDistance = distance;
                    bestResolutionIndex = i;
                }

                var item = selectionControl.Add($"{resolution.x}x{resolution.y}", () =>
                {
                    RegisterApplyButton(resolution, fullscreen);
                });
                Destroy(item.gameObject.GetComponent<LocalizeStringBehaviour>());
                Destroy(item.gameObject.GetComponent<LocalizeFontBehaviour>());
            }

            return bestResolutionIndex;
        }

        public void FullscreenSwitch(bool fullscreen)
        {
            List<Vector2Int> resolutions = fullscreen ? fullScreenResolutions : windowModeResolutions;
            Vector2Int preferredResolution = fullscreen ?
                new(fullScreenResolutions[^1].x, fullScreenResolutions[^1].y) :
                new(1920, 1080);

            var bestResolutionIndex = RefreshItems(resolutions, fullscreen, preferredResolution);

            RegisterApplyButton(resolutions[bestResolutionIndex], fullscreen);
            selectionControl.SelectImmediate(bestResolutionIndex, false);
        }

        private void RegisterApplyButton(Vector2Int resolution, bool fullscreen)
        {
            if (resolution == currentResolution && fullscreen == currentFullscreen)
            {
                applyButton.Interactable = false;
                return;
            }

            applyButton.onClick.RemoveAllListeners();
            applyButton.onClick.AddListener(() =>
            {
                var oldResolution = currentResolution;
                var oldFullscreen = currentFullscreen;
                GameObject confirmBox = Instantiate(confirmBoxPrefab, mainCanvas.transform);
                ResolutionChangeConfirmBox confirmBoxComponent = confirmBox.GetComponent<ResolutionChangeConfirmBox>();
                confirmBoxComponent.onYes.AddListener(() =>
                {
                    // do nothing
                });
                confirmBoxComponent.onNo.AddListener(() =>
                {
                    ChangeResolution(oldResolution, oldFullscreen);
                    currentResolution = oldResolution;
                    currentFullscreen = oldFullscreen;
                    applyButton.Interactable = true;
                });

                ChangeResolution(resolution, fullscreen);
                currentResolution = resolution;
                currentFullscreen = fullscreen;
                applyButton.Interactable = false;
            });
            applyButton.Interactable = true;
        }

        private void ChangeResolution(Vector2Int resolution, bool fullscreen)
        {
#if UNITY_EDITOR
            this.Log($"Fake change resolution to {resolution.x}x{resolution.y}, fullscreen: {fullscreen}");
#else
            Screen.SetResolution(resolution.x, resolution.y, fullscreen);
#endif
        }
    }
}