using System.Collections;
using System.Linq;
using Kaede2.AWS;
using Kaede2.Input;
using Kaede2.Scenario.Framework;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Localization.Settings;
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
            Application.targetFrameRate = ScenarioRunMode.Args.TestMode ? 60 : 1800;
            InputManager.EnsureInstance();
            AWSManager.Initialize();

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                typeof(SceneManager).Log($"Scene {scene.name} loaded with mode {mode:G}");
            };

            SceneManager.sceneUnloaded += scene =>
            {
                typeof(SceneManager).Log($"Scene {scene.name} unloaded");
            };

            // it seems that localization will try to initialize addressables with autoreleasehandle == true
            // which will cause the initialization handle to be disposed automatically by localization
            // changing the internal op version != handle version, making it an invalid handle
            // we skip addressables initialization and only wait for localization initialization for now
            // since localization will initialize addressables by itself
#if FALSE
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

            resourceLocator = handle.Result;
#endif

            typeof(GlobalInitializer).Log("Initializing localization");
            yield return LocalizationSettings.InitializationOperation;
            LocalizationSettings.Instance.SetSelectedLocale(GameSettings.Locale);
            typeof(GlobalInitializer).Log($"Changed language to {GameSettings.Locale}");

            CurrentStatus = Status.Done;
        }
    }
}