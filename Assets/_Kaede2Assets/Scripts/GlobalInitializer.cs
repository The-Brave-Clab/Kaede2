﻿using System.Collections;
using Kaede2.Input;
using UnityEngine;

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
            // yield return LocalizationSettings.InitializationOperation;
            InputManager.EnsureInstance();
            Application.targetFrameRate = 1800;

            CurrentStatus = Status.Done;
        }
    }
}