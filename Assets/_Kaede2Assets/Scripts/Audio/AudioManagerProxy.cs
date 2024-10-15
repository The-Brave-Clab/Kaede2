using Kaede2.ScriptableObjects;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2.Audio
{
    // this class exists because all APIs in AudioManager is static, which can't be added to serialized UnityEvent's
    public class AudioManagerProxy : MonoBehaviour
    {
        public void PlayBGM(string bgmName)
        {
            AudioManager.PlayBGM(bgmName);
        }

        public void PauseBGM()
        {
            AudioManager.PauseBGM();
        }

        public void ResumeBGM()
        {
            AudioManager.ResumeBGM();
        }

        public void ConfirmSound()
        {
            AudioManager.ConfirmSound();
        }

        public void CancelSound()
        {
            AudioManager.CancelSound();
        }

        public void ButtonSound()
        {
            AudioManager.ButtonSound();
        }

        public void MessageBoxSound()
        {
            AudioManager.MessageBoxSound();
        }

        public void PlayVoice(string voiceName, bool isCharacterVoice)
        {
            AudioManager.PlayVoice(voiceName, isCharacterVoice);
        }

        public void PlayRandomSystemVoice(int category)
        {
            AudioManager.PlayRandomSystemVoice((MasterSystemVoiceData.VoiceCategory) category);
        }
    }
}