using System.Collections;
using Kaede2.ScriptableObjects;
using Kaede2.UI.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class TitleMenuItem : SelectableItem
    {
        [SerializeField]
        private Image backgroundTop;

        [SerializeField]
        private Image backgroundBottom;

        [SerializeField]
        private Image overlay;

        [SerializeField]
        private TextMeshProUGUI text;

        protected override void Awake()
        {
            base.Awake();

            onSelectedChanged.AddListener(s =>
            {
                overlay.enabled = s;
                if (s)
                {
                    backgroundTop.color = Theme.Vol[GameSettings.ThemeVolume].MenuButtonHighlightTop;
                    backgroundBottom.color = Theme.Vol[GameSettings.ThemeVolume].MenuButtonHighlightBottom;
                    text.color = Color.white;
                    text.outlineColor = Theme.Vol[GameSettings.ThemeVolume].MenuButtonTextRim;
                    text.outlineWidth = 1;
                    text.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 1);
                }
                else
                {
                    backgroundTop.color = new Color(1, 1, 1, 0.902f);
                    backgroundBottom.color = new Color(0.808f, 0.812f, 0.808f, 0.988f);
                    text.color = Color.black;
                    text.outlineColor = Color.black;
                    text.outlineWidth = 0;
                    text.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0);
                }
                text.UpdateFontAsset();
            });
        }
    }
}