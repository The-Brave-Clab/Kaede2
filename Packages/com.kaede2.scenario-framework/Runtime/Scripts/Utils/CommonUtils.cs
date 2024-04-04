using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Utils
{
    public static class CommonUtils
    {
        #region CoroutineHelpers

        public static void InstantExecution(this IEnumerator enumerator)
        {
            try
            {
                while (enumerator.MoveNext())
                {
                    object current = enumerator.Current;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                if (ScenarioRunMode.Args.TestMode)
                {
                    ScenarioRunMode.FailTest(ScenarioRunMode.FailReason.Exception);
                }
            }
        }

        public static IEnumerator WithException(this IEnumerator enumerator)
        {
            while (true)
            {
                try
                {
                    if (!enumerator.MoveNext())
                    {
                        yield break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    if (ScenarioRunMode.Args.TestMode)
                    {
                        ScenarioRunMode.FailTest(ScenarioRunMode.FailReason.Exception);
                    }
                    yield break;
                }

                yield return enumerator.Current;
            }
        }

        #endregion

        #region DOTween

        public static Ease GetEase(string easeName)
        {
            return Enum.TryParse<Ease>(easeName, true, out var value) ? value : Ease.Linear;
        }

        #endregion

        #region LevenshteinDistance

        public static string FindClosestMatch(string input, IEnumerable<string> collection, out int distance)
        {
            distance = int.MaxValue;
            var closestMatch = "";

            foreach (var word in collection.OrderBy(w => w))
            {
                var currentDistance = CalculateLevenshteinDistance(input, word);
                if (currentDistance >= distance) continue;
                distance = currentDistance;
                closestMatch = word;
            }

            if (ScenarioRunMode.Args.TestMode && distance > 0)
            {
                ScenarioRunMode.FailTest(ScenarioRunMode.FailReason.BadParameter);
            }

            return closestMatch;
        }

        private static int CalculateLevenshteinDistance(string source, string target)
        {
            var n = source.Length;
            var m = target.Length;
            var distance = new int[n + 1][];

            for (var i = 0; i <= n; ++i)
            {
                distance[i] = new int[m + 1];
                distance[i][0] = i;
            }

            for (var j = 0; j <= m; ++j)
                distance[0][j] = j;

            for (var i = 1; i <= n; ++i)
            {
                for (var j = 1; j <= m; ++j)
                {
                    var cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    distance[i][j] = Math.Min(Math.Min(distance[i - 1][j] + 1, distance[i][j - 1] + 1), distance[i - 1][j - 1] + cost);
                }
            }

            return distance[n][m];
        }

        #endregion
    }
}