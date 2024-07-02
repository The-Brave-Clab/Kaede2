using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Kaede2
{
    public class AlbumTitle : MonoBehaviour
    {
        private static List<AlbumTitle> instances;

        [SerializeField]
        private TextMeshProUGUI text;

        public static string Text
        {
            get => (instances == null || instances.Count == 0) ? "" : instances[0].text.text;
            set
            {
                if (instances == null) return;
                foreach (var instance in instances)
                    instance.text.text = value;
            }
        }

        public static TMP_FontAsset Font
        {
            get => (instances == null || instances.Count == 0) ? null : instances[0].text.font;
            set
            {
                if (instances == null) return;
                foreach (var instance in instances.Where(instance => instance.text.font != value))
                {
                    instance.text.font = value;
                    instance.text.UpdateFontAsset();
                }
            }
        }

        private void Awake()
        {
            instances ??= new();
            instances.Add(this);
        }

        private void OnDestroy()
        {
            instances.Remove(this);
        }
    }
}
