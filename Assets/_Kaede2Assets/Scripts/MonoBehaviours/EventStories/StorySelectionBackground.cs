using System;
using System.Collections;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Kaede2
{
    public class StorySelectionBackground : MonoBehaviour
    {
        [SerializeField]
        private Sprite defaultBackground;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private AlbumExtraInfo albumExtraInfo;

        private AsyncOperationHandle<Sprite> currentBackgroundHandle;

        private Coroutine loadCoroutine;
        private string queuedRequest;

        private void Awake()
        {
            loadCoroutine = null;
            queuedRequest = null;
        }

        private void OnDestroy()
        {
            if (currentBackgroundHandle.IsValid())
            {
                Addressables.Release(currentBackgroundHandle);
            }
        }

        public void Set(string cardImageName)
        {
            if (loadCoroutine != null)
            {
                queuedRequest = cardImageName;
            }
            else
            {
                loadCoroutine = StartCoroutine(LoadCoroutine(cardImageName));
            }
        }

        private IEnumerator LoadCoroutine(string cardImageName)
        {
            if (string.IsNullOrEmpty(cardImageName))
            {
                backgroundImage.sprite = defaultBackground;
                yield break;
            }

            var lastBackgroundHandle = currentBackgroundHandle;

            currentBackgroundHandle = ResourceLoader.LoadIllustration(cardImageName);
            yield return currentBackgroundHandle;

            backgroundImage.sprite = currentBackgroundHandle.Status == AsyncOperationStatus.Succeeded ?
                currentBackgroundHandle.Result :
                defaultBackground;

            if (lastBackgroundHandle.IsValid())
            {
                Addressables.Release(lastBackgroundHandle);
            }

            loadCoroutine = null;
            if (queuedRequest != null)
            {
                var request = queuedRequest;
                queuedRequest = null;
                loadCoroutine = StartCoroutine(LoadCoroutine(request));
            }
        }
    }
}