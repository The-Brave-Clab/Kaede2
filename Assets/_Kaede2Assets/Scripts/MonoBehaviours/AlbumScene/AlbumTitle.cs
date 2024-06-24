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
