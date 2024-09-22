using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Kaede2
{
    public class CartoonEpisodeGroup : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField]
        private TextMeshProUGUI titleText;

        [SerializeField]
        private Image frame1;

        [SerializeField]
        private Image frame2;

        [SerializeField]
        private Image frame3;

        [SerializeField]
        private Image frame4;

        private List<AsyncOperationHandle<Sprite>> handles;
        private CartoonEpisodeSelection episodeSelection;

        private void OnDestroy()
        {
            Clear();
        }

        public IEnumerator Initialize(CartoonEpisodeSelection selection, string labelPrefix, MasterCartoonInfo.CartoonInfo info)
        {
            Clear();

            episodeSelection = selection;

            titleText.text = $"{labelPrefix} {info.CartoonLabel}";

            var handle1 = ResourceLoader.LoadCartoonFrame(info.ImageNames[0]);
            var handle2 = ResourceLoader.LoadCartoonFrame(info.ImageNames[1]);
            var handle3 = ResourceLoader.LoadCartoonFrame(info.ImageNames[2]);
            var handle4 = ResourceLoader.LoadCartoonFrame(info.ImageNames[3]);

            handles.Add(handle1);
            handles.Add(handle2);
            handles.Add(handle3);
            handles.Add(handle4);

            CoroutineGroup group = new();
            group.Add(handle1);
            group.Add(handle2);
            group.Add(handle3);
            group.Add(handle4);
            yield return group.WaitForAll();

            frame1.sprite = handle1.Result;
            frame2.sprite = handle2.Result;
            frame3.sprite = handle3.Result;
            frame4.sprite = handle4.Result;
        }

        private void Clear()
        {
            if (handles != null)
            {
                foreach (var handle in handles)
                {
                    Addressables.Release(handle);
                }
            }

            handles = new List<AsyncOperationHandle<Sprite>>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            episodeSelection.Select(this);
        }
    }
}