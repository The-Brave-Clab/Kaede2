using System;
using System.Collections;
using Kaede2.Utils;

namespace Kaede2.Scenario.Commands
{
    public class AliasText : Command
    {
        private readonly string aliasFileName;

        public AliasText(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            aliasFileName = Arg(1, "");
        }

        public override ExecutionType Type => Module.ScenarioResource.AliasText == null ? ExecutionType.Synchronous : ExecutionType.Instant;
        public override float ExpectedExecutionTime => -1;

        public override IEnumerator Execute()
        {
            if (Module.ScenarioResource.AliasText == null)
            {
                var aliasHandle = ResourceLoader.LoadScenarioAliasText(Module.ScenarioName, aliasFileName);
                Module.RegisterLoadHandle(aliasHandle);
                yield return aliasHandle.Send();

                Module.ScenarioResource.AliasText = aliasHandle.Result;
            }

            var aliasFileContent = Module.ScenarioResource.AliasText.text;
            string[] lines = aliasFileContent.Split('\n', '\r');
            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                trimmed = trimmed.Split(new[] {"//"}, StringSplitOptions.None)[0];
                if (trimmed == "") continue;
                string[] split = trimmed.Split('\t');
                string alias = split[0];
                string orig = split[1];
                Module.AddAlias(orig, alias);
            }
        }
    }
}