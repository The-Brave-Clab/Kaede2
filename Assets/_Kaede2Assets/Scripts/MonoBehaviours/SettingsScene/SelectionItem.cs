using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Kaede2
{
    public class SelectionItem : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        private UnityEvent onSelected;

        private RectTransform rt;

        public RectTransform RectTransform
        {
            get
            {
                if (rt == null)
                {
                    rt = GetComponent<RectTransform>();
                }

                return rt;
            }
        }

        public Color Color
        {
            get => text.color;
            set => text.color = value;
        }

        public string Text
        {
            get => text.text;
            set => text.text = value;
        }

        public void Select()
        {
            onSelected.Invoke();
        }
    }
}