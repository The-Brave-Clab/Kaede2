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
            ChangeTheme();
        }

        public void ChangeTheme()
        {
            OnThemeChange(Theme.Current);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, theme.MainTextRim);
            text.UpdateFontAsset();
        }
    }
}