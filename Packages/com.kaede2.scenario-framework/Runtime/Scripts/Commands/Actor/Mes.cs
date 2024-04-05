using System.Collections;
using Kaede2.Scenario.Framework.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class Mes : Command
    {
        private readonly string entityName;
        private readonly string speakerName;
        private readonly string voiceName;
        private readonly string message;

        private Live2DActorEntity entity;

        // ReSharper disable once MemberCanBeProtected.Global
        public Mes(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            var resourceSplit = OriginalArg(1).Split(':');
            if (resourceSplit.Length > 1)
            {
                entityName = resourceSplit[0];
                speakerName = resourceSplit[1];
            }
            else
            {
                entityName = "";
                speakerName = resourceSplit[0];
            }

            voiceName = Arg(2, "");
            message = OriginalArg(3);
        }

        public override ExecutionType Type => ExecutionType.Synchronous;
        public override float ExpectedExecutionTime => -10;

        public override void Setup()
        {
            entity = null;
            if (string.IsNullOrEmpty(entityName)) return;
            FindEntity(entityName, out entity);
        }

        public override IEnumerator Execute()
        {
            Module.OnMesCommand?.Invoke(speakerName, voiceName, message);
            var messageBox = Module.UIController.MessageBox;
            messageBox.gameObject.SetActive(true);
            messageBox.Message = message;
            messageBox.NameTag = speakerName;

            // this is needed to guard the situation where the last message
            // ends on the same frame as the current message starts
            bool lastMesClicked = Module.MesClicked;

            if (!AudioManager.IsInvalidVoice(voiceName))
                Module.AudioManager.PlayVoice(voiceName);

            float extraTimeAfterTextFinishDisplay = 1.0f;

            float skipTimeInTestMode = 0.2f;

            while (true)
            {
                if (Module.LipSync && entity != null)
                {
                    entity.SetLip(Module.AudioManager.GetVoiceVolume());
                }

                bool currentMesClicked = Module.MesClicked;
                if (currentMesClicked && !lastMesClicked)
                {
                    if (!messageBox.IsCompleteDisplayText) // if text not finished displaying, skip display
                        messageBox.SkipDisplay();
                    else
                        break; // manual quit is only available when text is finished displaying
                }
                lastMesClicked = currentMesClicked;

                yield return null;

                if (ScenarioRunMode.Args.TestMode)
                {
                    skipTimeInTestMode -= Time.deltaTime;
                    if (skipTimeInTestMode <= 0)
                        break;
                }

                // when text is finished displaying, start counting down for extra time
                if (messageBox.IsCompleteDisplayText)
                    extraTimeAfterTextFinishDisplay -= Time.deltaTime;

                // auto quit in auto mode
                if (!Module.AudioManager.IsVoicePlaying() && extraTimeAfterTextFinishDisplay <= 0 && Module.AutoMode)
                    break;
            }

            if (entity != null)
            {
                if (Module.LipSync)
                {
                    entity.SetLip(0);
                }
                entity.RemoveAllMouthSync();
            }
            Module.AudioManager.StopVoice();
        }
    }
}