using System.Collections;
using Kaede2.Input;
using Kaede2.Scenario.Audio;
using Kaede2.Scenario.Base;
using Kaede2.Scenario.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class Mes : Command
    {
        private readonly string entityName;
        private readonly string speakerName;
        private readonly string voiceName;
        private readonly string message;

        private Live2DActorEntity entity;

        // ReSharper disable once MemberCanBeProtected.Global
        public Mes(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
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

        public override IEnumerator Setup()
        {
            entity = null;
            if (!string.IsNullOrEmpty(entityName))
            {
                FindEntity(entityName, out entity);
            }
            yield break;
        }

        public override IEnumerator Execute()
        {
            var messageBox = Module.UIController.MessageBox;
            messageBox.gameObject.SetActive(true);
            messageBox.Text = message;
            messageBox.nameTag.text = speakerName;

            if (!AudioManager.IsInvalidVoice(voiceName))
                Module.AudioManager.PlayVoice(voiceName);

            float extraTimeAfterTextFinishDisplay = 1.0f;

            var lastFrameButtonPressed = true;
            while (true)
            {
                if (Module.LipSync && entity != null)
                {
                    entity.SetLip(Module.AudioManager.GetVoiceVolume() * 128);
                }

                var currentFrameButtonPressed = InputManager.InputAction.Scenario.Next.triggered;

                if (currentFrameButtonPressed && !lastFrameButtonPressed)
                {
                    if (!messageBox.IsCompleteDisplayText) // if text not finished displaying, skip display
                        messageBox.SkipDisplay();
                    else
                        break; // manual quit is only available when text is finished displaying
                }

                lastFrameButtonPressed = currentFrameButtonPressed;

                yield return null;

                // when text is finished displaying, start counting down for extra time
                if (messageBox.IsCompleteDisplayText)
                    extraTimeAfterTextFinishDisplay -= Time.deltaTime;

                bool autoMode = false; // TODO

                // auto quit in auto mode
                if (!Module.AudioManager.IsVoicePlaying() && extraTimeAfterTextFinishDisplay <= 0 && autoMode)
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