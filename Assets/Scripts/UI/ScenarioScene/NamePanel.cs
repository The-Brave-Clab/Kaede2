using TMPro;
using UnityEngine;

namespace Kaede2.UI.ScenarioScene
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class NamePanel : MonoBehaviour
    {
        private RectTransform rt;

        public TextMeshProUGUI text;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
        }

        void Update()
        {
            rt.sizeDelta = new Vector2(
                Mathf.Max(400, text.preferredWidth + 140f), 
                68f);
        }
    }
}
