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
        private TextMeshProUGUI tmpText;

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
                tmpText.text = $"{text[..1]}<color=black>{text[1..]}</color>";
            }
            else
            {
                tmpText.text = "";
            }
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            adjustHSV.adjustment = theme.InterfaceTitleBackground;
        }
    }
}