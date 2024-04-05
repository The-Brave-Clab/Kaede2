using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class GyuukiLoading : MonoBehaviour
    {
        [SerializeField]
        private Image gyuuki;

        [SerializeField]
        private Sprite[] gyuukiSprites;

        [SerializeField]
        private float spinTime = 0.1f;

        [SerializeField]
        private RectTransform[] loadingTexts;

        [SerializeField]
        private float waveHeight = 25;

        [SerializeField]
        private float waveWidth = 50;

        [SerializeField]
        private float waveSpeed = 10;

        [SerializeField]
        private float waveInterval = 1.0f;

        private Coroutine spinCoroutine;
        private Coroutine waveCoroutine;
        private void OnEnable()
        {
            spinCoroutine = StartCoroutine(SpinGyuuki());
            waveCoroutine = StartCoroutine(WaveLoadingTexts());
        }

        private void OnDisable()
        {
            if (spinCoroutine != null)
            {
                StopCoroutine(spinCoroutine);
            }

            if (waveCoroutine != null)
            {
                StopCoroutine(waveCoroutine);
            }
        }

        private IEnumerator SpinGyuuki()
        {
            while (gameObject != null)
            {
                foreach (var s in gyuukiSprites)
                {
                    gyuuki.sprite = s;
                    yield return new WaitForSeconds(spinTime);
                }
            }
        }

        private IEnumerator WaveLoadingTexts()
        {
            // get width bound of LoadingTexts
            var widthMin = float.MaxValue;
            var widthMax = float.MinValue;
            foreach (var t in loadingTexts)
            {
                var left = t.anchoredPosition.x - t.sizeDelta.x / 2;
                var right = t.anchoredPosition.x + t.sizeDelta.x / 2;
                widthMax = Mathf.Max(widthMax, right);
                widthMin = Mathf.Min(widthMin, left);
            }
            
            while (gameObject != null)
            {
                float Wave(float x)
                {
                    var waveSlope = 2 * waveHeight / waveWidth;
                    var halfWaveWidth = waveWidth / 2;
                    if (x < -halfWaveWidth || x > halfWaveWidth)
                    {
                        return 0;
                    }

                    if (x < 0) return waveSlope * x + waveHeight;
                    return -waveSlope * x + waveHeight;
                }

                var startTime = Time.time;
                var waveStartPosition = widthMin - waveWidth / 2;
                var waveEndPosition = widthMax + waveWidth / 2;

                var waveCurrentPosition = waveStartPosition;
                while (waveCurrentPosition < waveEndPosition)
                {
                    var waveTime = Time.time - startTime;
                    waveCurrentPosition = waveStartPosition + waveSpeed * waveTime;
                    foreach (var t in loadingTexts)
                    {
                        var x = t.anchoredPosition.x;
                        var y = Wave(x - waveCurrentPosition);
                        t.anchoredPosition = new Vector2(x, y);
                    }
                    yield return null;
                }

                yield return new WaitForSeconds(waveInterval);
            }
        }
    }
}
