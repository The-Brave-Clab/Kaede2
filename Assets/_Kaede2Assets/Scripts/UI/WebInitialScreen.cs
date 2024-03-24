using System.Collections;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Kaede2.UI
{
    public class WebInitialScreen : MonoBehaviour
    {
        [SerializeField]
        private Image unityLogo;

        [SerializeField]
        private Image webglLogo;

        [SerializeField]
        private Image webgpuLogo;

        [SerializeField]
        private Image live2dLogo;

#if UNITY_WEBGL && !UNITY_EDITOR
        private void Awake()
        {
            unityLogo.color = new Color(1, 1, 1, 0);
            webglLogo.color = new Color(1, 1, 1, 0);
            webgpuLogo.color = new Color(1, 1, 1, 0);
            live2dLogo.color = new Color(1, 1, 1, 0);

            bool isWebGPU = SystemInfo.graphicsDeviceType == GraphicsDeviceType.WebGPU;
            webglLogo.gameObject.GetComponent<Transform>().parent.gameObject.SetActive(!isWebGPU);
            webgpuLogo.gameObject.GetComponent<Transform>().parent.gameObject.SetActive(isWebGPU);
        }

        private IEnumerator Start()
        {
            WebInterop.EnsureInstance();

            CoroutineGroup group = new CoroutineGroup();
            group.Add(ShowLogos(), this);
            group.Add(LoadScenarioMasterData(), this);
            group.Add(GlobalInitializer.Initialize(), this);
            yield return group.WaitForAll();
        }

        private IEnumerator ShowLogos()
        {
            // skip one frame to hide the webgl/webgpu logo change
            yield return null;

            unityLogo.color = Color.white;
            webglLogo.color = Color.white;
            webgpuLogo.color = Color.white;
            live2dLogo.color = Color.white;
        }

        private IEnumerator LoadScenarioMasterData()
        {
            using var handle = ResourceLoader.LoadMasterData<MasterScenarioInfo>();
            yield return handle.Send();
            if (handle.Result == null)
            {
                Debug.LogError("Failed to load scenario master data");
                yield break;
            }
            WebInterop.OnScenarioListLoaded(JsonUtility.ToJson(handle.Result));
        }
#endif
    }
}
