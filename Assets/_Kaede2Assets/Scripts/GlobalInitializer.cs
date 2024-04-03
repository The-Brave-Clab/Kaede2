using System.Collections;
using Kaede2.AWS;
using Kaede2.Input;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
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
            Application.targetFrameRate = 1800;
            InputManager.EnsureInstance();
            AWSManager.Initialize();

            var handle = Addressables.InitializeAsync(false);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Failed)
            {
                CurrentStatus = Status.Failed;
                yield break;
            }

            resourceLocator = handle.Result;

            // yield return LocalizationSettings.InitializationOperation;

            CurrentStatus = Status.Done;
        }
    }
}