﻿using System.Collections;
using Kaede2.Scenario.Framework.Entities;
using Kaede2.Scenario.Framework.Live2D;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Kaede2
{
    public class Live2DTestSpawner : MonoBehaviour
    {
        [SerializeField]
        private Camera renderCamera;

        [SerializeField]
        private Canvas renderCanvas;

        [SerializeField]
        private GameObject uiEmptyPrefab;

        [SerializeField]
        private string modelName;

        private AsyncOperationHandle<Live2DAssets> loadHandle;

        private IEnumerator Start()
        {
            if (renderCamera == null || renderCanvas == null || string.IsNullOrEmpty(modelName)) yield break;

            if (renderCamera.GetComponent<Live2DRenderer>() == null)
            {
                renderCamera.gameObject.AddComponent<Live2DRenderer>();
            }

            loadHandle = ResourceLoader.LoadLive2DModel(modelName);
            yield return loadHandle;

            GameObject newModel = Instantiate(uiEmptyPrefab, renderCanvas.transform, false);
            newModel.transform.localPosition = Vector3.zero;
            Live2DActorEntity entity = newModel.AddComponent<Live2DActorEntity>();
            entity.CreateWithAssets(loadHandle.Result);
            entity.Hidden = false;
        }

        private void OnDestroy()
        {
            Addressables.Release(loadHandle);
        }
    }
}