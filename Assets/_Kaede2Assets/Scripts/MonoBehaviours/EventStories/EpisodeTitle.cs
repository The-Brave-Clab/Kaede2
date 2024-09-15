using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class EpisodeTitle : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI text;

        private string titleText;
        private string labelText;

        public string Label
        {
            get => labelText;
            set
            {
                labelText = value;
                UpdateText();
            }
        }

        public string Text
        {
            get => titleText;
            set
            {
                titleText = value;
                UpdateText();
            }
        }

        private void UpdateText()
        {
            text.text = $" <size=83.33%>{labelText} </size> {titleText}";
            text.UpdateFontAsset();
        }
    }
}