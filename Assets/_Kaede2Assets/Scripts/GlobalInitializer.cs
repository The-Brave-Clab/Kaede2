using System.Collections;
using Kaede2.Input;
using Kaede2.Scenario.Framework;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace Kaede2
{
    public static class GlobalInitializer
    {
        public enum Status
        {
            NotStarted,
            InProgress,
            Done,
            Failed,
        }

        public static Status CurrentStatus { get; private set; } = Status.NotStarted;

        public static IEnumerator Initialize()
        {
            if (CurrentStatus != Status.NotStarted)
            {
                // somebody else is already initializing
                // just wait for it to finish and quit
                while (CurrentStatus != Status.Done)
                {
                    yield return null;
                }

                yield break;
            }

            CurrentStatus = Status.InProgress;

            // Initialize all the things here
#if UNITY_STANDALONE || UNITY_EDITOR
            Application.targetFrameRate = ScenarioRunMode.Args.TestMode ? 60 : -1;
            QualitySettings.vSyncCount = 0;
#else
            Application.targetFrameRate = 120;
            QualitySettings.vSyncCount = 1;
#endif

            InputManager.EnsureInstance();

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                typeof(SceneManager).Log($"Scene {scene.name} loaded with mode {mode:G}");
            };

            SceneManager.sceneUnloaded += scene =>
            {
                typeof(SceneManager).Log($"Scene {scene.name} unloaded");
            };

            typeof(GlobalInitializer).Log("Initializing Addressables");
            var handle = Addressables.InitializeAsync(false);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Failed)
            {
                CurrentStatus = Status.Failed;
                typeof(GlobalInitializer).LogError("Failed to initialize Addressables");
                yield break;
            }
            typeof(GlobalInitializer).Log("Addressables initialized");

            typeof(GlobalInitializer).Log("Initialization complete");

            CurrentStatus = Status.Done;
        }
    }
}