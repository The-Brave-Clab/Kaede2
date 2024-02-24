using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Kaede2.Assets.AssetBundles;
using Kaede2.Utils;

namespace Kaede2
{
    public class SplashInitializer : MonoBehaviour
    {
        [SerializeField]
        private List<Image> _splashSprites;

        [SerializeField]
        private float _delayDuration = 0.5f;

        [SerializeField]
        private float _fadeDuration = 0.5f;

        [SerializeField]
        private float _splashDuration = 2.0f;

        private void Awake()
        {
            foreach (var image in _splashSprites)
            {
                image.color = new Color(1, 1, 1, 0);
            }
        }

        private void SetSplashSpritesColor(Color c)
        {
            foreach (var image in _splashSprites)
            {
                image.color = c;
            }
        }

        private IEnumerator SplashColor()
        {
            yield return new WaitForSeconds(_delayDuration);

            float currentTime = Time.time;

            while (Time.time - currentTime < _fadeDuration)
            {
                SetSplashSpritesColor(new Color(1, 1, 1, Mathf.Clamp01((Time.time - currentTime) / _fadeDuration)));
                yield return null;
            }
            
            SetSplashSpritesColor(new Color(1, 1, 1, 1));

            yield return new WaitForSeconds(_splashDuration);

            currentTime = Time.time;
            
            while (Time.time - currentTime < _fadeDuration)
            {
                SetSplashSpritesColor(new Color(1, 1, 1, Mathf.Clamp01(1 - (Time.time - currentTime) / _fadeDuration)));

                yield return null;
            }

            SetSplashSpritesColor(new Color(1, 1, 1, 0));
        }

        private IEnumerator Start()
        {
            CoroutineGroup coroutineGroup = new CoroutineGroup();
            coroutineGroup.Add(SplashColor());
            coroutineGroup.Add(AssetBundleManifestData.LoadManifest());
            yield return coroutineGroup.WaitForAll();

            // for testing only
            ResourceLoader loader = new ResourceLoader();
            yield return loader.LoadAsync<TextAsset>("scenario/es001_001_m001_a/es001_001_m001_a_script.txt",
                textAsset => Debug.Log(textAsset.text),
                progress => Debug.Log($"Loading progress: {progress * 100}%"));
        }
    }
}