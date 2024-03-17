using TMPro;
using UnityEngine;

namespace Kaede2.Scenario.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class NamePanel : MonoBehaviour
    {
        [SerializeField]
        private float minWidth = 400f;

        [SerializeField]
        private float additionalWidth = 140f;

        [SerializeField]
        private float height = 68f;

        private RectTransform rt;

        public TextMeshProUGUI text;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
        }

        void Update()
        {
            rt.sizeDelta = new Vector2(
                Mathf.Max(minWidth, text.preferredWidth + additionalWidth), 
                height);
        }
    }
}
