using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Scenario.Commands;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2.Scenario
{
    public partial class ScenarioModule
    {
        private IEnumerator PreloadIncludeFiles(List<string> statements, Dictionary<string, List<string>> includeFiles)
        {
            var includeStatements = statements.Where(s => s.StartsWith("include")).ToList();

            List<Tuple<string, ResourceLoader.LoadAddressableHandle<TextAsset>>> includeHandles = new();
            foreach (var s in includeStatements)
            {
                string[] args = s.Split(new[] { '\t' }, StringSplitOptions.None);
                string includeFileName = args[1];
                if (includeFileName == "define_function") includeFileName = "define_functions"; // a fix
                // for now the include files are only in defines
                var includeHandle = ResourceLoader.LoadScenarioDefineText(includeFileName);
                includeHandles.Add(new(includeFileName, includeHandle));
            }

            if (includeHandles.Count == 0)
                yield break;

            CoroutineGroup group = new();
            foreach (var (_, handle) in includeHandles)
                group.Add(handle.Send(), this);
            yield return group.WaitForAll();

            foreach (var (fileName, handle) in includeHandles)
            {
                var includeFileContent = handle.Result.text;
                // include/define files are in a self-contained bundle
                // since we are not going to use them after this, it's ok to release the handles
                handle.Dispose();

                Debug.Log($"Pre-Loaded include file {fileName}");
                var includeFileStatements = GetStatementsFromScript(includeFileContent);
                includeFiles[fileName] = includeFileStatements;
                yield return PreloadIncludeFiles(includeFileStatements, includeFiles);
            }
        }

        private static List<string> PreprocessInclude(List<string> originalStatements,
            Dictionary<string, List<string>> includeFiles)
        {
            List<string> outputStatements = new();

            foreach (var s in originalStatements)
            {
                if (!s.StartsWith("include"))
                {
                    outputStatements.Add(s);
                    continue;
                }

                string[] args = s.Split(new[] { '\t' }, StringSplitOptions.None);
                string includeFileName = args[1];
                if (includeFileName == "define_function") includeFileName = "define_functions"; // a fix
                var includeStatements = includeFiles[includeFileName];
                var processedIncludeStatements = PreprocessInclude(includeStatements, includeFiles);
                outputStatements.AddRange(processedIncludeStatements);
            }

            return outputStatements;
        }

        private class Function
        {
            public readonly string FunctionName;
            private readonly List<string> statements;
            private readonly List<Parameter> parameters;

            private class Parameter
            {
                public readonly int Index;
                public readonly string Name;

                public Parameter(int index, string name)
                {
                    Index = index;
                    Name = name;
                }
            }

            public Function(string definitionStatement)
            {
                var trimmed = definitionStatement.Trim();
                var split = trimmed.Split('\t');

                FunctionName = split[1];

                parameters = new List<Parameter>(split.Length - 2);
                statements = new List<string>();

                for (int i = 2; i < split.Length; ++i)
                {
                    parameters.Add(new Parameter(i - 2, split[i]));
                }
            }

            public void FinishDefinition()
            {
                parameters.Sort((p2, p1) => p1.Name.Length.CompareTo(p2.Name.Length));
            }

            public void AddStatement(string statement)
            {
                statements.Add(statement);
            }

            public List<string> GetStatements(List<string> parameterValues)
            {
                if (parameterValues.Count != parameters.Count)
                {
                    Debug.LogError($"Error calling function {FunctionName}! Number of desired parameters ({parameterValues.Count}) doesn't fit with defined ({parameters.Count}).");
                    return null;
                }

                List<string> realStatements = new List<string>(statements.Count);

                foreach (var statement in statements)
                {
                    string realStatement = statement;

                    for (int i = 0; i < parameters.Count; ++i)
                    {
                        foreach (var parameter in parameters.Where(parameterPair => parameterPair.Index == i))
                        {
                            realStatement = realStatement.Replace(parameter.Name, parameterValues[i]);
                            break;
                        }
                    }

                    realStatements.Add(realStatement);
                }

                return realStatements;
            }
        }

        private static List<string> PreprocessFunctions(List<string> statements)
        {
            Dictionary<string, Function> functions = new();
            Function currentFunction = null;
            bool recordingFunction = false;

            List<string> outputStatements = new List<string>();

            foreach (var s in statements)
            {
                // if recording function, just add to current function
                if (recordingFunction && !s.StartsWith("endfunction"))
                {
                    currentFunction.AddStatement(s);
                    continue;
                }

                // if recording and we should end, finish recording
                if (recordingFunction && s.StartsWith("endfunction"))
                {
                    currentFunction.FinishDefinition();
                    functions.Add(currentFunction.FunctionName, currentFunction);

                    currentFunction = null;
                    recordingFunction = false;
                    continue;
                }

                // if not recording and we should start, start recording
                if (s.StartsWith("function"))
                {
                    currentFunction = new Function(s);
                    recordingFunction = true;
                    continue;
                }

                // if not recording and we should call a function, call it
                if (s.StartsWith("sub"))
                {
                    var split = s.Split('\t');
                    var functionName = split[1];
                    var parameters = new List<string>(split.Length - 2);
                    for (int i = 2; i < split.Length; ++i)
                    {
                        parameters.Add(split[i]);
                    }

                    if (!functions.ContainsKey(functionName))
                    {
                        Debug.LogError($"Function {functionName} doesn't exist!");
                        continue;
                    }

                    Function f = functions[functionName];
                    var functionStatements = f.GetStatements(parameters);

                    outputStatements.AddRange(functionStatements);
                    continue;
                }

                // if not recording and we should do something else, just add it
                outputStatements.Add(s);
            }

            return outputStatements;
        }

        private IEnumerator PreprocessAliasesAndVariables(List<string> statements)
        {
            foreach (var s in statements)
            {
                if (s.StartsWith("alias_text"))
                {
                    if (ParseStatement(s) is AliasText command)
                        yield return ExecuteSingle(command);
                }
                else if (s.StartsWith("set"))
                {
                    if (ParseStatement(s) is Set command)
                        command.Execute().InstantExecution();
                }
            }
        }
    }
}