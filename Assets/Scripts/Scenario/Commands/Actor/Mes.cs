using System.Collections;
using Kaede2.Input;
using Kaede2.Scenario.Audio;
using Kaede2.Scenario.Entities;
using Kaede2.Scenario.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kaede2.Scenario.Commands
{
    public class Mes : ScenarioModule.Command
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
            var messageBox = UIManager.Instance.MessageBox;
            messageBox.gameObject.SetActive(true);
            messageBox.SetText(message);
            messageBox.nameTag.text = speakerName;

            if (!AudioManager.IsInvalidVoice(voiceName))
                AudioManager.Instance.PlayVoice(voiceName);

            float extraTimeAfterTextFinishDisplay = 1.0f;

            while (true)
            {
                if (Module.LipSync && entity != null)
                {
                    entity.SetLip(AudioManager.Instance.GetVoiceVolume() * 128);
                }

                if (InputManager.InputAction.Scenario.Next.triggered)
                {
                    if (!messageBox.IsCompleteDisplayText) // if text not finished displaying, skip display
                        messageBox.SkipDisplay();
                    else
                        break; // manual quit is only available when text is finished displaying
                }

                yield return null;

                // when text is finished displaying, start counting down for extra time
                if (messageBox.IsCompleteDisplayText)
                    extraTimeAfterTextFinishDisplay -= Time.deltaTime;

                bool autoMode = false; // TODO

                // auto quit in auto mode
                if (!AudioManager.Instance.IsVoicePlaying() && extraTimeAfterTextFinishDisplay <= 0 && autoMode)
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
            AudioManager.Instance.StopVoice();
        }
    }
}