using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2.Scenario
{
    public partial class ScenarioModule
    {
        public abstract class Command
        {
            public enum ExecutionType
            {
                Instant,
                Synchronous,
                Asynchronous
            }

            private readonly string[] args;
            private readonly string[] originalArgs;

            protected readonly ScenarioModule Module;

            public abstract ExecutionType Type { get; }

            public Command(ScenarioModule module, string[] arguments)
            {
                Module = module;
                args = arguments;
                originalArgs = new string[arguments.Length];
                Array.Copy(arguments, originalArgs, arguments.Length);

                for (int i = 0; i < args.Length; i++)
                {
                    string resolved = Module.ResolveAlias(args[i]);
                    if (resolved != null)
                        args[i] = resolved;
                }
            }

            public override string ToString()
            {
                string result = "";
                foreach (var s in args)
                {
                    result += s + "\t";
                }

                return result.Trim();
            }

            protected T Arg<T>(int index, T defaultValue = default)
            {
                try
                {
                    if (index >= args.Length) return defaultValue;
                    if (typeof(T) == typeof(string)) return (T)(object)args[index];
                    if (typeof(T) == typeof(bool)) return (T)(object)bool.Parse(args[index]);
                    if (typeof(T) == typeof(Ease)) return (T)(object)CommonUtils.GetEase(args[index]);
                    return Module.Evaluate<T>(args[index]);
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"Cannot parse Arg[{index}] = {args[index]} as {typeof(T).Name}. Using default value {defaultValue}.\n{e.Message}");
                    return defaultValue;
                }
            }

            protected string OriginalArg(int index, string defaultValue = "")
            {
                return index >= originalArgs.Length ? defaultValue : originalArgs[index];
            }

            protected static T FindEntity<T>(string name) where T : Entity
            {
                var entities = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                var substituteName =
                    CommonUtils.FindClosestMatch(name, entities.Select(e => e.gameObject.name), out var distance);
                var result = entities.First(e => e.gameObject.name == substituteName);
                if (distance != 0)
                    Debug.LogWarning(
                        $"{typeof(T).Name} '{name}' doesn't exist, using '{substituteName}' instead. Distance is {distance}.");
                return result;
            }

            public virtual IEnumerator Setup()
            {
                yield break;
            }

            public abstract IEnumerator Execute();
        }
    }
}