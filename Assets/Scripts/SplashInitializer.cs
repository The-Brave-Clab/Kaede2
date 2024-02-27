using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Kaede2.Utils;

namespace Kaede2
{
    public class SplashInitializer : MonoBehaviour
    {
        [SerializeField]
        private List<Image> splashSprites;

        [SerializeField]
        private float delayDuration = 0.5f;

        [SerializeField]
        private float fadeDuration = 0.5f;

        [SerializeField]
        private float splashDuration = 2.0f;

        private void Awake()
        {
            foreach (var image in splashSprites)
            {
                image.color = new Color(1, 1, 1, 0);
            }
        }

        private IEnumerator Start()
        {
            yield return SplashColor();

            using var live2DHandler = ResourceLoader.LoadLive2DModel("adv_01yy01_moc_01");
            yield return live2DHandler.Send();

            var live2DModel = live2DHandler.Result;
            Debug.Log($"Live2D model loaded with {live2DModel.motionFiles.Count} motions");

            yield return new WaitForSeconds(10);
            Debug.Log("Releasing handle");
        }

        private void SetSplashSpritesColor(Color c)
        {
            foreach (var image in splashSprites)
            {
                image.color = c;
            }
        }

        private IEnumerator SplashColor()
        {
            yield return new WaitForSeconds(delayDuration);

            float currentTime = Time.time;

            while (Time.time - currentTime < fadeDuration)
            {
                var a = Mathf.Clamp01((Time.time - currentTime) / fadeDuration);
                a = Mathf.Pow(a, 4);
                SetSplashSpritesColor(new Color(1, 1, 1, a));
                yield return null;
            }
            
            SetSplashSpritesColor(new Color(1, 1, 1, 1));

            yield return new WaitForSeconds(splashDuration);

            currentTime = Time.time;
            
            while (Time.time - currentTime < fadeDuration)
            {
                var a = Mathf.Clamp01(1 - (Time.time - currentTime) / fadeDuration);
                a = Mathf.Pow(a, 4);
                SetSplashSpritesColor(new Color(1, 1, 1, a));

                yield return null;
            }

            SetSplashSpritesColor(new Color(1, 1, 1, 0));
        }
    }
}