using System.Collections;
using System.Collections.Generic;
using Kaede2.Input;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Kaede2
{
    public class SplashScreen : MonoBehaviour
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

            InputManager.InputAction.SplashScreen.Enable();
#if UNITY_IOS
            UnityEngine.iOS.Device.hideHomeButton = true;
#endif
        }

        private IEnumerator Start()
        {
            yield return SplashColor();

            CommonUtils.LoadNextScene("OpeningMovieScene", LoadSceneMode.Single);
        }

        private void OnDestroy()
        {
            InputManager.InputAction?.SplashScreen.Disable();
#if UNITY_IOS
            UnityEngine.iOS.Device.hideHomeButton = false;
#endif
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
            float currentTime = Time.time;

            bool shouldSkip = false;

            while (Time.time - currentTime < delayDuration)
            {
                if (shouldSkip) break;
                shouldSkip = InputManager.InputAction.SplashScreen.Skip.triggered;

                yield return null;
            }

            currentTime = Time.time;

            while (Time.time - currentTime < fadeDuration)
            {
                if (shouldSkip) break;
                shouldSkip = InputManager.InputAction.SplashScreen.Skip.triggered;

                var a = Mathf.Clamp01((Time.time - currentTime) / fadeDuration);
                a = Mathf.Pow(a, 4);
                SetSplashSpritesColor(new Color(1, 1, 1, a));
                yield return null;
            }
            
            SetSplashSpritesColor(new Color(1, 1, 1, 1));

            shouldSkip = false;

            while (Time.time - currentTime < splashDuration)
            {
                if (shouldSkip) break;
                shouldSkip = InputManager.InputAction.SplashScreen.Skip.triggered;

                yield return null;
            }

            currentTime = Time.time;
            
            while (Time.time - currentTime < fadeDuration)
            {
                if (shouldSkip) break;
                shouldSkip = InputManager.InputAction.SplashScreen.Skip.triggered;

                var a = Mathf.Clamp01(1 - (Time.time - currentTime) / fadeDuration);
                a = Mathf.Pow(a, 4);
                SetSplashSpritesColor(new Color(1, 1, 1, a));

                yield return null;
            }

            SetSplashSpritesColor(new Color(1, 1, 1, 0));
        }
    }
}