using TMPro;
using UnityEngine;

namespace Kaede2
{
    public class AlbumTitle : MonoBehaviour
    {
        private static AlbumTitle instance;

        [SerializeField]
        private TextMeshProUGUI text;

        public static string Text
        {
            get => instance == null ? "" : instance.text.text;
            set
            {
                if (instance != null) instance.text.text = value;
            }
        }

        public static TMP_FontAsset Font
        {
            get => instance == null ? null : instance.text.font;
            set
            {
                if (instance == null) return;
                if (instance.text.font == value) return;

                instance.text.font = value;
                instance.text.UpdateFontAsset();
            }
        }

        private void Awake()
        {
            instance = this;
        }

        private void OnDestroy()
        {
            instance = null;
        }
    }
}
