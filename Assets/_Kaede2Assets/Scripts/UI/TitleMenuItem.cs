using System;
using Kaede2.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI
{
    [ExecuteAlways]
    public class TitleMenuItem : MonoBehaviour
    {
        [SerializeField]
        private Image backgroundTop;

        [SerializeField]
        private Image backgroundBottom;

        [SerializeField]
        private Image overlay;

        [SerializeField]
        private TextMeshProUGUI text;

        public bool Selected;
        private bool lastSelected;

        private void Update()
        {
            if (Selected == lastSelected) return;
            lastSelected = Selected;

            overlay.enabled = Selected;
            backgroundTop.color = Selected ? Theme.Vol[GameSettings.ThemeVolume].MenuButtonHighlightTop : new Color(1, 1, 1, 0.902f);
            backgroundBottom.color = Selected ? Theme.Vol[GameSettings.ThemeVolume].MenuButtonHighlightBottom : new Color(0.808f, 0.812f, 0.808f, 0.988f);
            text.color = Selected ? Color.white : Color.black;
            text.outlineColor = Selected ? Theme.Vol[GameSettings.ThemeVolume].MenuButtonTextRim : Color.black;
            text.outlineWidth = Selected ? 1 : 0;
            text.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, Selected ? 1 : 0);
            text.UpdateFontAsset();
        }
    }
}