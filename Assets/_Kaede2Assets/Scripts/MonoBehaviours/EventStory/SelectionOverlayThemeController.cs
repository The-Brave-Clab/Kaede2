using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;

namespace Kaede2
{
    public class SelectionOverlayThemeController : MonoBehaviour, IThemeChangeObserver
    {
        [SerializeField]
        private AdjustHSV adjustHSV;

        private void Awake()
        {
            OnThemeChange(Theme.Current);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            adjustHSV.adjustment = theme.SelectionOverlay;
        }
    }
}