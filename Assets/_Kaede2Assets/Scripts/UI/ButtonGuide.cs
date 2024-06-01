using Kaede2.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kaede2.UI
{
    public class ButtonGuide : MonoBehaviour, IThemeChangeObserver, IPointerClickHandler
    {
        [SerializeField]
        private Image image;

        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float alpha;

        [SerializeField]
        private Color textColor;

        [SerializeField]
        private UnityEvent onClick;

        private void Awake()
        {
            OnThemeChange(Theme.Current);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            var themeColor = theme.ButtonGuideColor;
            image.color = new Color(themeColor.r, themeColor.g, themeColor.b, alpha);
            text.color = textColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Invoke();
        }

        public void Invoke()
        {
            onClick.Invoke();
        }
    }
}
