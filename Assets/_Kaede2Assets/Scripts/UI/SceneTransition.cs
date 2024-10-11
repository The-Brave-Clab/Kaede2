using System.Collections;
using DG.Tweening;
using Kaede2.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI
{
    public class SceneTransition : MonoBehaviour
    {
        [Range(0, 1)]
        [SerializeField]
        private float progress;

        [SerializeField]
        private RectTransform mainCanvas;

        [SerializeField]
        [ColorUsage(false, false)]
        private Color color;

        [SerializeField]
        private RawImage filler;

        [SerializeField]
        private Image gradient;

        private RectTransform rt;

        private static SceneTransition _instance;

        private void Awake()
        {
            rt = (RectTransform) transform;
            DontDestroyOnLoad(mainCanvas.gameObject);
            progress = 0;
            _instance = this;
        }

        private void Update()
        {
            filler.color = new Color(color.r, color.g, color.b, 1);
            gradient.color = new Color(color.r, color.g, color.b, 1);

            var pixelRect = mainCanvas.rect;

            float length = pixelRect.width + pixelRect.height;
            float right = Mathf.Lerp(-length, length, progress);
            rt.offsetMin = new Vector2(-pixelRect.height, 0);
            rt.offsetMax = new Vector2(right, 0);
        }

        public static void SetColor(Color color)
        {
            if (_instance == null) return;
            _instance.color = new Color(color.r, color.g, color.b, _instance.color.a);
        }

        public static IEnumerator Fade(float targetProgress, float time = 0.5f)
        {
            // in some situation we don't have the instance
            if (_instance == null) yield break;

            float startProgress = _instance.progress;
            Sequence seq = DOTween.Sequence();
            seq.Append(DOVirtual.Float(startProgress, targetProgress, time, value => { _instance.progress = value; }));
            yield return seq.WaitForCompletion();
        }
    }
}
