using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;

namespace Kaede2
{
    public class SelectionOverlayThemeController : MonoBehaviour, IThemeChangeObserver
    {
        private AdjustHSV adjustHSV;

        private void Awake()
        {
            adjustHSV = GetComponent<AdjustHSV>();
            OnThemeChange(Theme.Current);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            adjustHSV.adjustment = theme.SelectionOverlay;
        }
    }
}