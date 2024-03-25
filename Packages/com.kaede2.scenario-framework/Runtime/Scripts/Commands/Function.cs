using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class Function
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
                Debug.LogError(
                    $"Error calling function {FunctionName}! Number of desired parameters ({parameterValues.Count}) doesn't fit with defined ({parameters.Count}).");
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

        public static List<string> PreprocessFunctions(List<string> statements)
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
    }
}