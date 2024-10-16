using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Kaede2
{
    public class SupporterTextController : MonoBehaviour
    {
        private Supporters supporters;

        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        private string emptyNameSubstitute;

        [SerializeField]
        private float scrollTime = 20;

        private void Awake()
        {
            supporters = Supporters.GetInstance();
            string supporterText = "";
            foreach (var (key, value) in supporters.SupportersPrioritized)
            {
                string current = "";
                foreach (var val in value)
                {
                    var supporter = val;
                    if (string.IsNullOrEmpty(supporter))
                        supporter = emptyNameSubstitute;

                    supporterText += supporter + "\n";
                }

                supporterText += current + "\n\n";
            }

            text.text = supporterText;
        }

        private IEnumerator Start()
        {
            yield return null;

            var rectTransform = transform as RectTransform;
            if (rectTransform == null) yield break;

            var textHeight = text.rectTransform.rect.height;
            var containerHeight = rectTransform.rect.height;

            var startY = 0;
            var endY = textHeight + containerHeight;
            StartScroll(startY, endY);
        }

        private void StartScroll(float startY, float endY)
        {
            StartCoroutine(ScrollCoroutine(startY, endY));
        }

        private IEnumerator ScrollCoroutine(float startY, float endY)
        {
            var time = 0f;
            while (time < scrollTime)
            {
                time += Time.deltaTime;
                var t = time / scrollTime;
                var y = Mathf.Lerp(startY, endY, t);
                text.rectTransform.anchoredPosition = new Vector2(0, y);
                yield return null;
            }

            text.rectTransform.anchoredPosition = new Vector2(0, endY);
            yield return new WaitForSeconds(2);
            StartScroll(startY, endY);
        }
    }
}