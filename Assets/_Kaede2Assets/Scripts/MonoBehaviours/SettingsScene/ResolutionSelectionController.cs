using System;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2
{
    public class ResolutionSelectionController : MonoBehaviour
    {
        [SerializeField]
        private SelectionControl selectionControl;

        [SerializeField]
        private SelectionControl fullscreenControl;

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

            var bestResolutionIndex = RefreshItems(windowModeResolutions, Screen.fullScreen, new Vector2Int(Screen.width, Screen.height));
            selectionControl.SelectImmediate(bestResolutionIndex, false);

            fullscreenControl.SelectImmediate(Screen.fullScreen ? 1 : 0, false);
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

                selectionControl.Add($"{resolution.x}x{resolution.y}", () =>
                {
                    // Screen.SetResolution(resolution.x, resolution.y, fullscreen);
                    this.Log($"Fake change resolution to {resolution.x}x{resolution.y}, fullscreen: {fullscreen}");
                });
            }

            return bestResolutionIndex;
        }

        public void FullscreenSwitch(bool fullscreen)
        {
            List<Vector2Int> resolutions = fullscreen ? fullScreenResolutions : windowModeResolutions;
            Vector2Int preferredResolution = fullscreen ?
                // we have not entered fullscreen yet, so Screen.currentResolution should be the native resolution
                new(Screen.currentResolution.width, Screen.currentResolution.height) :
                new(1920, 1080);

            var bestResolutionIndex = RefreshItems(resolutions, fullscreen, preferredResolution);

            // Screen.SetResolution(resolutions[bestResolutionIndex].x, resolutions[bestResolutionIndex].y, fullscreen);
            this.Log($"Fake change resolution to {resolutions[bestResolutionIndex].x}x{resolutions[bestResolutionIndex].y}, fullscreen: {fullscreen}");
            selectionControl.SelectImmediate(bestResolutionIndex, false);
        }
    }
}