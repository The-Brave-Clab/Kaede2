using Kaede2.ScriptableObjects;
using Kaede2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class FilterBanner : MonoBehaviour, IThemeChangeObserver
    {
        [SerializeField]
        private Image background;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private TextMeshProUGUI text;

        private void Awake()
        {
            OnThemeChange(Theme.Current);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            background.color = theme.FavGradientTop;
            SetFontOutlineColor(theme);
        }

        public void ChangeFont(TMP_FontAsset fontAsset)
        {
            text.font = fontAsset;
            SetFontOutlineColor(Theme.Current);
        }

        private void SetFontOutlineColor(Theme.VolumeTheme theme)
        {
            text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, theme.MainTextRim);
            text.UpdateFontAsset();
        }
    }
}
