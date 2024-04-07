using System.Collections;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Utils
{
    public class CoroutineProxy : SingletonMonoBehaviour<CoroutineProxy>
    {
        protected override void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }

        public static Coroutine Start(IEnumerator routine)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return null;
#endif
            return Instance.StartCoroutine(routine);
        }

        public static void Stop(Coroutine routine)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            Instance.StopCoroutine(routine);
        }
    }
}