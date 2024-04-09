using Kaede2.ScriptableObjects;
using Kaede2.UI;
using TMPro;
using UnityEngine;

namespace Kaede2
{
    public class MainMenuTextRim : MonoBehaviour, IThemeChangeObserver
    {
        [SerializeField]
        private TextMeshProUGUI text;

        private void Awake()
        {
            OnThemeChange(Theme.Current);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            text.outlineColor = theme.MainTextRim;
            text.UpdateFontAsset();
        }
    }
}