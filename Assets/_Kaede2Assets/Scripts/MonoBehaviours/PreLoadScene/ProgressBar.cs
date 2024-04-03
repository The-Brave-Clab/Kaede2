using System;
using Kaede2.Utils;
using TMPro;
using UnityEngine;

namespace Kaede2
{
    [ExecuteAlways]
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField]
        private RectTransform progressBarFill;

        [SerializeField]
        private float padding = 2;

        [SerializeField]
        private TextMeshProUGUI percentageText;

        [SerializeField]
        private TextMeshProUGUI progressText;

        [SerializeField]
        private TextMeshProUGUI speedText;

        private long lastValue;
        private float lastValueTime;
        private long value;

        private long maxValue = 100;

        private RectTransform rectTransform;

        private void Awake()
        {
            if (!Application.isPlaying) return;
            percentageText.text = "";
            progressText.text = "";
            speedText.text = "";
        }

        private void Start()
        {
            lastValueTime = Time.time;
            lastValue = value;
        }

        private void Update()
        {
            if (progressBarFill == null) return;
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            var backgroundSize = rectTransform.sizeDelta;
            padding = Mathf.Clamp(padding, 0, backgroundSize.y / 2);
            progressBarFill.offsetMin = Vector2.one * padding;
            progressBarFill.offsetMax =
                new Vector2(
                    Mathf.Lerp(padding - backgroundSize.x + 24, -padding, Mathf.InverseLerp(0, maxValue, value)),
                    -padding);
        }

        
        public void SetValue(long currentValue, long totalValue)
        {
            maxValue = totalValue;
            value = currentValue;

            percentageText.text = $"{currentValue / (float) totalValue * 100:F1}%";
            progressText.text = $"{CommonUtils.BytesToHumanReadable(currentValue)} / {CommonUtils.BytesToHumanReadable(totalValue)}";

            var time = Time.time - lastValueTime;
            if (time < 0.5f) return;
            var downloadedBytes = currentValue - lastValue;
            var speed = downloadedBytes / time;
            speedText.text = $"{CommonUtils.BytesToHumanReadable(speed)}/s";

            lastValueTime = Time.time;
            lastValue = currentValue;
        }
    }
}