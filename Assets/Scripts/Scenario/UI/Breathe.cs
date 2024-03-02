using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.Scenario.UI
{
    [RequireComponent(typeof(Image))]
    public class Breathe : MonoBehaviour
    {
        private Image image = null;
        private Coroutine breatheCoroutine = null;

        public float periodInSeconds = 1.0f;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private IEnumerator BreatheCoroutine()
        {
            Color color = image.color;
            float t = 0.0f;
            while (true)
            {
                color.a = Mathf.Clamp01(Mathf.Sqrt(2 * Mathf.Abs(t - Mathf.Floor(t + 0.5f))) * 1.5f);
                image.color = color;

                t += Time.deltaTime / periodInSeconds;
                yield return null;
            }
        }

        private void OnEnable()
        {
            Color color = image.color;
            color.a = 0;
            image.color = color;

            breatheCoroutine = StartCoroutine(BreatheCoroutine());
        }

        private void OnDisable()
        {
            Color color = image.color;
            color.a = 0;
            image.color = color;

            if (breatheCoroutine != null)
                StopCoroutine(breatheCoroutine);
        }
    }
}