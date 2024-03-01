using System;
using System.Collections;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2.Scenario
{
    public partial class ScenarioModule
    {
        private IEnumerator Execute()
        {
            while (true)
            {
                ++currentCommandIndex;
                if (currentCommandIndex >= commands.Count)
                {
                    yield break;
                }

                var command = commands[currentCommandIndex];
                var execution = ExecuteSingle(command);
                while (execution.MoveNext())
                {
                    yield return execution.Current;
                }
            }
        }

        public IEnumerator ExecuteSingle(Command command)
        {
#if UNITY_EDITOR
            var args = command.ToString().Split('\t');
            Debug.Log($"<color=#00FF00>[{Time.frameCount}]</color>\t<color=#FFFF00>{args[0]}</color>\t<color=#7777FF>{string.Join('\t', args[1..])}</color>");
#endif

            switch (command.Type)
            {
                case Command.ExecutionType.Instant:
                {
                    command.Setup().InstantExecution();
                    command.Execute().InstantExecution();
                    break;
                }
                case Command.ExecutionType.Synchronous:
                {
                    var execution = SyncExecution(command);
                    while (execution.MoveNext())
                    {
                        yield return execution.Current;
                    }
                    break;
                }
                case Command.ExecutionType.Asynchronous:
                {
                    StartCoroutine(SyncExecution(command));
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IEnumerator SyncExecution(Command command)
        {
            IEnumerator setup = command.Setup();
            while (setup.MoveNext())
            {
                yield return setup.Current;
            }
            IEnumerator execute = command.Execute();
            while (execute.MoveNext())
            {
                yield return execute.Current;
            }
        }
    }
}