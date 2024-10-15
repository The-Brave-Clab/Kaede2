using System.Collections;
using System.Linq;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

namespace Kaede2.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Sound Effect Clips")]
        [SerializeField]
        private AudioClip confirm;

        [SerializeField]
        private AudioClip cancel;

        [SerializeField]
        private AudioClip button;

        [SerializeField]
        private AudioClip messageBox;

        [Header("Audio Sources")]
        [SerializeField]
        private AudioSource bgmSource;

        [SerializeField]
        private AudioSource seSource;

        [SerializeField]
        private AudioSource voiceSource;

        private static AudioManager _instance;

        private string currentPlayingBGM;
        private AsyncOperationHandle<AudioClip> bgmHandle;

        private string loadedVoiceName;
        private AsyncOperationHandle<AudioClip> voiceHandle;

        private float bgmVolume;
        private float seVolume;
        private float voiceVolume;

        private void Awake()
        {
            if (_instance != null)
            {
                this.LogWarning("AudioManager already exists. Destroying this instance.");
                Destroy(this);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            bgmVolume = 1;
            seVolume = 1;
            voiceVolume = 1;
        }

        private void OnDestroy()
        {
            if (bgmHandle.IsValid())
            {
                Addressables.Release(bgmHandle);
            }

            if (voiceHandle.IsValid())
            {
                Addressables.Release(voiceHandle);
            }

            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Update()
        {
            bgmSource.volume = GameSettings.AudioBGMVolume * GameSettings.AudioMasterVolume * bgmVolume;
            seSource.volume = GameSettings.AudioSEVolume * GameSettings.AudioMasterVolume * seVolume;
            voiceSource.volume = GameSettings.AudioVoiceVolume * GameSettings.AudioMasterVolume * voiceVolume;
        }

        public static void PlayBGM(string bgmName)
        {
            if (_instance == null) return;

            CoroutineProxy.Start(_instance.PlayBGMInternal(bgmName));
        }

        private IEnumerator PlayBGMInternal(string bgmName)
        {
            if (currentPlayingBGM == bgmName) yield break;
            currentPlayingBGM = bgmName;

            if (!bgmHandle.IsDone)
            {
                yield return bgmHandle;
            }

            if (bgmHandle.IsValid())
            {
                Addressables.Release(bgmHandle);
            }

            bgmHandle = ResourceLoader.LoadSystemBackgroundMusic(bgmName);

            yield return bgmHandle;

            if (bgmSource.isPlaying)
            {
                yield return FadeBGM(0);
                bgmSource.Stop();
            }

            bgmSource.clip = bgmHandle.Result;
            bgmSource.Play();
        }

        public static void PauseBGM()
        {
            if (_instance == null) return;

            IEnumerator PauseCoroutine()
            {
                yield return _instance.FadeBGM(0);
                _instance.bgmSource.Pause();
            }

            CoroutineProxy.Start(PauseCoroutine());
        }

        public static void ResumeBGM()
        {
            if (_instance == null) return;

            _instance.bgmSource.Play();
            CoroutineProxy.Start(_instance.FadeBGM(1));
        }

        private IEnumerator FadeBGM(float targetVolume, float time = 0.5f)
        {
            float startVolume = bgmVolume;
            float startTime = Time.time;

            while (Time.time < startTime + time)
            {
                bgmVolume = Mathf.Lerp(startVolume, targetVolume, (Time.time - startTime) / time);
                yield return null;
            }

            bgmVolume = targetVolume;
        }

        public static void ConfirmSound()
        {
            if (_instance == null) return;

            _instance.seSource.PlayOneShot(_instance.confirm);
        }

        public static void CancelSound()
        {
            if (_instance == null) return;

            _instance.seSource.PlayOneShot(_instance.cancel);
        }

        public static void ButtonSound()
        {
            if (_instance == null) return;

            _instance.seSource.PlayOneShot(_instance.button);
        }

        public static void MessageBoxSound()
        {
            if (_instance == null) return;

            _instance.seSource.Stop();
            _instance.seSource.clip = _instance.messageBox;
            _instance.seSource.Play();
        }

        public static void PlayVoice(string voiceName, bool isCharacterVoice)
        {
            if (_instance == null) return;

            if (!isCharacterVoice && _instance.voiceSource.isPlaying) // we don't overlap/overwrite system voices
                return;

            CoroutineProxy.Start(_instance.PlayVoiceInternal(voiceName, isCharacterVoice));
        }

        private IEnumerator PlayVoiceInternal(string voiceName, bool isCharacterVoice)
        {
            if (loadedVoiceName != voiceName)
            {
                if (!voiceHandle.IsDone)
                {
                    yield return voiceHandle;
                }

                if (voiceHandle.IsValid())
                {
                    Addressables.Release(voiceHandle);
                }

                voiceHandle = isCharacterVoice
                    ? ResourceLoader.LoadCharacterVoice(voiceName)
                    : ResourceLoader.LoadSystemVoice(voiceName);

                yield return voiceHandle;
            }

            voiceSource.Stop();
            voiceSource.clip = voiceHandle.Result;
            voiceSource.Play();
        }

        public static void PlayRandomSystemVoice(MasterSystemVoiceData.VoiceCategory category)
        {
            if (_instance == null) return;

            // we still do this check here even if PlayVoice already does it
            // just to save some calculations
            if (_instance.voiceSource.isPlaying) return;

            var voice = MasterSystemVoiceData.Instance.Data
                .Where(vd => vd.categoryId == category)
                .OrderBy(_ => Random.value)
                .FirstOrDefault();

            if (voice == null) // which should never happen
            {
                _instance.LogError($"No voice found for category {category:G}");
                return;
            }

            PlayVoice(voice.cueName, false);
        }
        
    }
}