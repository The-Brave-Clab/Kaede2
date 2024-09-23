using System;
using System.Collections;
using System.Collections.Generic;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Kaede2
{
    public class CartoonViewWindow : MonoBehaviour
    {
        [SerializeField]
        private BoxWindow window;

        [SerializeField]
        private Scrollbar scrollbar;

        [SerializeField]
        private CommonButton backButton;

        [SerializeField]
        private Image frame1;

        [SerializeField]
        private Image frame2;

        [SerializeField]
        private Image frame3;

        [SerializeField]
        private Image frame4;

        private List<AsyncOperationHandle<Sprite>> handles;

        private void OnDestroy()
        {
            Clear();
        }

        public IEnumerator Initialize(MasterCartoonInfo.CartoonInfo cartoonInfo)
        {
            Clear();

            var handle1 = ResourceLoader.LoadCartoonFrame(cartoonInfo.ImageNames[0]);
            var handle2 = ResourceLoader.LoadCartoonFrame(cartoonInfo.ImageNames[1]);
            var handle3 = ResourceLoader.LoadCartoonFrame(cartoonInfo.ImageNames[2]);
            var handle4 = ResourceLoader.LoadCartoonFrame(cartoonInfo.ImageNames[3]);

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

            window.TitleText = cartoonInfo.CartoonLabel;

            scrollbar.value = 1;
        }

        private void Clear()
        {
            if (handles != null)
            {
                foreach (var handle in handles)
                {
                    if (handle.IsValid())
                        handle.Release();
                }
            }
            handles = new();
        }
    }
}
