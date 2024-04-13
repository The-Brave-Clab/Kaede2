using Kaede2.ScriptableObjects;
using Kaede2.UI;
using TMPro;
using UnityEngine;

namespace Kaede2
{
    [ExecuteAlways]
    public class InterfaceTitle : MonoBehaviour, IThemeChangeObserver
    {
        [SerializeField]
        private AdjustHSV adjustHSV;

        [SerializeField]
        private TextMeshProUGUI firstCharacterText;

        [SerializeField]
        private TextMeshProUGUI restOfText;

        [SerializeField]
        [TextArea(3, 10)]
        private string text;

        public string Text { set => text = value; }

        private void Awake()
        {
            if (Application.isPlaying)
                OnThemeChange(Theme.Current);
        }

        private void Update()
        {
            if (text.Length > 1)
            {
                var firstChar = text[..1];
                var rest = text[1..];

                firstCharacterText.text = $"{firstChar}";
                restOfText.text = $"<color=#FFF0>{firstChar}</color>{rest}";
            }
            else
            {
                firstCharacterText.text = "";
                restOfText.text = "";
            }
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            adjustHSV.adjustment = theme.InterfaceTitleBackground;
        }
    }
}