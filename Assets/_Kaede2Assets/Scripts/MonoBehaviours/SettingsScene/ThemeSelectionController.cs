using System;
using UnityEngine;

namespace Kaede2
{
    public class ThemeSelectionController : MonoBehaviour
    {
        [SerializeField]
        private SelectionControl selectionControl;

        private void Awake()
        {
            selectionControl.SelectImmediate(GameSettings.ThemeVolume + 1, false);
        }

        // used by SelectionItem UnityEvent
        public void SelectTheme(int themeIndex)
        {
            GameSettings.ThemeVolume = themeIndex;
        }
    }
}