using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                string[] args = s.Split(new[] {'\t'}, StringSplitOptions.None);
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
                handle.Dispose();
                Debug.Log($"Pre-Loaded include file {fileName}");
                var includeFileStatements = GetStatementsFromScript(includeFileContent);
                includeFiles[fileName] = includeFileStatements;
                yield return PreloadIncludeFiles(includeFileStatements, includeFiles);
            }
        }

        private static List<string> PreprocessInclude(List<string> originalStatements, Dictionary<string, List<string>> includeFiles)
        {
            List<string> outputStatements = new();

            foreach (var s in originalStatements)
            {
                if (!s.StartsWith("include"))
                {
                    outputStatements.Add(s);
                    continue;
                }

                string[] args = s.Split(new[] {'\t'}, StringSplitOptions.None);
                string includeFileName = args[1];
                if (includeFileName == "define_function") includeFileName = "define_functions"; // a fix
                var includeStatements = includeFiles[includeFileName];
                var processedIncludeStatements = PreprocessInclude(includeStatements, includeFiles);
                outputStatements.AddRange(processedIncludeStatements);
            }

            return outputStatements;
        }
    }
}