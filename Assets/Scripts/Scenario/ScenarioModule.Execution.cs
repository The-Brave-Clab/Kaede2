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

                Debug.Log($"[{Time.frameCount}]\t{command}");

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
                        var execution = SyncExecution();
                        while (execution.MoveNext())
                        {
                            yield return execution.Current;
                        }
                        break;
                    }
                    case Command.ExecutionType.Asynchronous:
                    {
                        StartCoroutine(SyncExecution());
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                continue;

                IEnumerator SyncExecution()
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
    }
}