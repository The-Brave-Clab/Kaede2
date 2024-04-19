using System.Collections;
using Kaede2.AWS;
using Kaede2.Input;
using Kaede2.Scenario.Framework;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

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

        private static IResourceLocator resourceLocator;
        public static IResourceLocator ResourceLocator => CurrentStatus != Status.Done ? null : resourceLocator;

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

            typeof(GlobalInitializer).Log("Initializing localization");
            yield return LocalizationSettings.InitializationOperation;
            LocalizationSettings.Instance.SetSelectedLocale(GameSettings.Locale);
            typeof(GlobalInitializer).Log($"Changed language to {GameSettings.Locale}");

            CurrentStatus = Status.Done;
        }
    }
}