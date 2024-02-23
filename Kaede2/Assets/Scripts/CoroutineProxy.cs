using System.Collections;
using UnityEngine;

namespace Kaede2
{
    public static class CoroutineProxy
    {
        private static readonly GameObject ProxyObject;

        static CoroutineProxy()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            ProxyObject = new GameObject("CoroutineProxy");
            Object.DontDestroyOnLoad(ProxyObject);
        }

        public static Coroutine StartCoroutine(IEnumerator routine)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return null;
#endif
            return ProxyObject.GetComponent<MonoBehaviour>().StartCoroutine(routine);
        }

        public static void StopCoroutine(Coroutine routine)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            ProxyObject.GetComponent<MonoBehaviour>().StopCoroutine(routine);
        }
    }
}