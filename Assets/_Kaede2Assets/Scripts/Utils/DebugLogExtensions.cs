using System;
using System.Diagnostics;
using UnityEngine;
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
        private static void Log(LogType logType, object obj, object message)
        {
           if (obj is Object unityObj)
                Debug.unityLogger.Log(logType, (object) $"[{unityObj.GetType().Name}] {message}", unityObj);
           else
                Debug.unityLogger.Log(logType, $"[{obj.GetType().Name}] {message}");
        }

        [Conditional(CompileCondition)]
        private static void Log(LogType logType, Type type, object message)
        {
            Debug.unityLogger.Log(logType, $"[{type.Name}] {message}");
        }

        [Conditional(CompileCondition)]
        public static void Log(this object obj, object message)
        {
            Log(LogType.Log, obj, message);
        }

        [Conditional(CompileCondition)]
        public static void Log(this Type type, object message)
        {
            Log(LogType.Log, type, message);
        }

        [Conditional(CompileCondition)]
        public static void LogWarning(this object obj, string message)
        {
            Log(LogType.Warning, obj, message);
        }

        [Conditional(CompileCondition)]
        public static void LogWarning(this Type type, string message)
        {
            Log(LogType.Warning, type, message);
        }

        [Conditional(CompileCondition)]
        public static void LogError(this object obj, string message)
        {
            Log(LogType.Error, obj, message);
        }

        [Conditional(CompileCondition)]
        public static void LogError(this Type type, string message)
        {
            Log(LogType.Error, type, message);
        }
    }
}