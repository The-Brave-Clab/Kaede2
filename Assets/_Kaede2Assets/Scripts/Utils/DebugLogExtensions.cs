using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Kaede2.Utils
{
    public static class DebugLogExtensions
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string CompileCondition = "UNITY_5_3_OR_NEWER";
#else
        private const string CompileCondition = "ALWAYS_FALSE";
#endif

        [Conditional(CompileCondition)]
        public static void Log(this object obj, string message)
        {
            if (obj is Object unityObj)
                Debug.Log($"[{unityObj.GetType().Name}] {message}", unityObj);
            else
                Debug.Log($"[{obj.GetType().Name}] {message}");
        }

        [Conditional(CompileCondition)]
        public static void Log(this Type type, string message)
        {
            Debug.Log($"[{type.Name}] {message}");
        }

        [Conditional(CompileCondition)]
        public static void LogWarning(this object obj, string message)
        {
            if (obj is Object unityObj)
                Debug.LogWarning($"[{unityObj.GetType().Name}] {message}", unityObj);
            else
                Debug.LogWarning($"[{obj.GetType().Name}] {message}");
        }

        [Conditional(CompileCondition)]
        public static void LogWarning(this Type type, string message)
        {
            Debug.LogWarning($"[{type.Name}] {message}");
        }

        [Conditional(CompileCondition)]
        public static void LogError(this object obj, string message)
        {
            if (obj is Object unityObj)
                Debug.LogError($"[{unityObj.GetType().Name}] {message}", unityObj);
            else
                Debug.LogError($"[{obj.GetType().Name}] {message}");
        }

        [Conditional(CompileCondition)]
        public static void LogError(this Type type, string message)
        {
            Debug.LogError($"[{type.Name}] {message}");
        }
    }
}