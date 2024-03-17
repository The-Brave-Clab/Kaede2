using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kaede2.Scenario
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
    }
}