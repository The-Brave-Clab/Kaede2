using Kaede2.Scenario.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.Scenario.Framework.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class CaptionBox : MonoBehaviour, IStateSavable<CaptionState>
    {
        public TextMeshProUGUI text;
        public Image box;

        private RectTransform rt;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
        }

        void Update()
        {
            var vector = new Vector2(text.preferredWidth, text.preferredHeight);
            text.rectTransform.sizeDelta = vector;
            rt.sizeDelta = vector * 1.3f;
        }

        public CaptionState GetState()
        {
            return new()
            {
                enabled = gameObject.activeSelf,
                boxColor = box.color,
                text = text.text,
                textAlpha = text.color.a
            };
        }

        public void RestoreState(CaptionState state)
        {
            gameObject.SetActive(state.enabled);
            box.color = state.boxColor;
            text.text = state.text;
            var color = text.color;
            color.a = state.textAlpha;
            text.color = color;
        }
    }
}