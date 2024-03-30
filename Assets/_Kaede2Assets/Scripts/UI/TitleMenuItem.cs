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
        private Image background;

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
            background.color = Selected ? Theme.Vol[GameSettings.ThemeVolume].menuButtonHighlight : Color.white;
            text.color = Selected ? Color.white : Color.black;
            text.outlineColor = Selected ? Theme.Vol[GameSettings.ThemeVolume].menuButtonTextRim : Color.black;
            text.outlineWidth = Selected ? 1 : 0;
            text.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, Selected ? 1 : 0);
            text.UpdateFontAsset();
        }
    }
}