﻿using System.Collections;
using Kaede2.Input;

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
                yield break;
            }

            CurrentStatus = Status.InProgress;

            // Initialize all the things here
            InputManager.EnsureInstance();

            CurrentStatus = Status.Done;
        }

        public static IEnumerator Wait()
        {
            while (CurrentStatus == Status.InProgress)
            {
                yield return null;
            }
        }
    }
}