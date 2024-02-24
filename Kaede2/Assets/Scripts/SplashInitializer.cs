﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Kaede2.Assets.AssetBundles;
using Kaede2.Utils;
using UnityEngine.Serialization;

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
            CoroutineGroup coroutineGroup = new CoroutineGroup();
            coroutineGroup.Add(SplashColor(), this);
            coroutineGroup.Add(AssetBundleManifestData.LoadManifest(), this);
            yield return coroutineGroup.WaitForAll();
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