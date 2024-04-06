using System.Collections;
using Kaede2.Scenario.Framework.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    // it looks like that this command will not be added to the message log
    public class VoicePlay : Command
    {
        private readonly string entityName;
        private readonly string voiceName;
        private readonly bool lipSync; // from the scenarios, this will always be false
        private readonly bool stopOtherVoices; // this will always be true

        private Live2DActorEntity entity;

        public VoicePlay(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            entityName = OriginalArg(1);
            voiceName = Arg(2, "");
            lipSync = Arg(3, true);
            stopOtherVoices = Arg(4, true);
        }

        public override ExecutionType Type => ExecutionType.Asynchronous;
        public override float ExpectedExecutionTime => -1;

        public override void Setup()
        {
            entity = null;
            if (string.IsNullOrEmpty(entityName)) return;
            FindEntity(entityName, out entity);
        }

        public override IEnumerator Execute()
        {
            // backup the original lip sync value
            bool originalLipSync = Module.LipSync;
            Module.LipSync = lipSync;

            // our implementation always stops other voices so we don't use the stopOtherVoices parameter
            // it will always be true anyway
            if (!AudioManager.IsInvalidVoice(voiceName))
                Module.AudioManager.PlayVoice(voiceName);

            // wait until the voice is finished playing
            yield return new WaitUntil(() => !Module.AudioManager.IsVoicePlaying());

            // restore the original lip sync value
            Module.LipSync = originalLipSync;
        }
    }
}