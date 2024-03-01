using System;
using System.Collections;
using Kaede2.Utils;

namespace Kaede2.Scenario.Commands
{
    public class AliasText : ScenarioModule.Command
    {
        private readonly string aliasFileName;

        public AliasText(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            aliasFileName = Arg(1, "");
        }

        public override ExecutionType Type => Module.ScenarioResource.aliasText == null ? ExecutionType.Synchronous : ExecutionType.Instant;
        public override float ExpectedExecutionTime => -1;

        public override IEnumerator Execute()
        {
            if (Module.ScenarioResource.aliasText == null)
            {
                var aliasHandle = ResourceLoader.LoadScenarioAliasText(ScenarioModule.ScenarioName, aliasFileName);
                Module.RegisterLoadHandle(aliasHandle);
                yield return aliasHandle.Send();

                Module.ScenarioResource.aliasText = aliasHandle.Result;
            }

            var aliasFileContent = Module.ScenarioResource.aliasText.text;
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

        public override void Undo()
        {
            // it's okay to not remove the variable, because it will be overwritten
        }
    }
}