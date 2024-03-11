using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.Audio;

namespace Kaede2.Scenario.Audio
{
    public class AudioManager : Singleton<AudioManager>, IStateSavable<AudioState>
    {
        [SerializeField]
        private AudioMixerGroup bgmMixerGroup;

        [SerializeField]
        private AudioMixerGroup seMixerGroup;

        [SerializeField]
        private AudioMixerGroup voiceMixerGroup;

        private AudioInfo bgmAudioInfo;
        private AudioInfo voiceAudioInfo;
        private List<AudioInfo> seAudioInfos;

        private bool running;

        protected override void Awake()
        {
            base.Awake();

            bgmAudioInfo = null;
            voiceAudioInfo = null;
            seAudioInfos = new();

            running = true;
        }

        private void Update()
        {
            if (bgmAudioInfo != null)
            {
                if (IsDead(bgmAudioInfo))
                {
                    Destroy(bgmAudioInfo);
                    bgmAudioInfo = null;
                }
            }

            if (voiceAudioInfo != null)
            {
                if (IsDead(voiceAudioInfo))
                {
                    Destroy(voiceAudioInfo);
                    voiceAudioInfo = null;
                }
            }

            List<AudioInfo> toBeRemoved = new();
            foreach (var seAudioInfo in seAudioInfos)
            {
                if (IsDead(seAudioInfo))
                {
                    Destroy(seAudioInfo);
                    toBeRemoved.Add(seAudioInfo);
                }
            }

            foreach (var info in toBeRemoved)
            {
                seAudioInfos.Remove(info);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            running = !pauseStatus;
        }

        public void PlayBGM(string bgmName, float volume)
        {
            StopAudioImmediately(bgmAudioInfo);
            bgmAudioInfo = null;

            if (!ScenarioModule.Instance.ScenarioResource.backgroundMusics.TryGetValue(bgmName, out var clip))
            {
                Debug.LogError($"BGM {bgmName} not found");
                return;
            }
            bgmAudioInfo = Create(bgmName, AudioType.BGM, clip, volume);
            bgmAudioInfo.Source.Play();
        }

        private void StopAudioImmediately(AudioInfo info)
        {
            if (info == null) return;
            if (info.Source == null) return;
            info.Source.Stop();
            Destroy(info);
        }

        public IEnumerator StopBGM(float fadeTime)
        {
            if (fadeTime > 0)
            {
                if (bgmAudioInfo == null) return null;
                if (bgmAudioInfo.Source == null) return null;
                var originalBgmAudioInfo = bgmAudioInfo;
                bgmAudioInfo = null;
                return Fade(originalBgmAudioInfo, fadeTime, originalBgmAudioInfo.Volume, 0, () =>
                {
                    StopAudioImmediately(originalBgmAudioInfo);
                    originalBgmAudioInfo = null;
                });
            }

            StopAudioImmediately(bgmAudioInfo);
            bgmAudioInfo = null;
            return null;
        }

        public void PlayVoice(string voiceName)
        {
            StopVoice();

            if (IsInvalidVoice(voiceName))
                return;

            if (!ScenarioModule.Instance.ScenarioResource.voices.TryGetValue(voiceName, out var clip))
            {
                Debug.LogError($"Voice {voiceName} not found");
                return;
            }

            voiceAudioInfo = Create(voiceName, AudioType.Voice, clip, 1.0f);
            voiceAudioInfo.Source.Play();
        }

        public void StopVoice()
        {
            StopAudioImmediately(voiceAudioInfo);
            voiceAudioInfo = null;
        }

        public float GetVoiceVolume()
        {
            if (voiceAudioInfo == null) return 0.0f;
            if (voiceAudioInfo.Source == null) return 0.0f;
            if (!voiceAudioInfo.Source.isPlaying) return 0.0f;
            return GetCurrentVolume(voiceAudioInfo.Source);
        }

        public bool IsVoicePlaying()
        {
            return !IsDead(voiceAudioInfo);
        }


        private float GetCurrentVolume(AudioSource audio)
        {
            const int fftSize = 128;
            float[] data = new float[fftSize];
            float sum = 0;
            audio.GetSpectrumData(data, 0, FFTWindow.Rectangular);
            foreach (float s in data)
            {
                sum += Mathf.Abs(s);
            }

            return sum / fftSize;
        }

        public IEnumerator PlaySE(string seName, float volume, float duration, bool loop)
        {
            if (!ScenarioModule.Instance.ScenarioResource.soundEffects.TryGetValue(seName, out var clip))
            {
                Debug.LogError($"SE {seName} not found");
                return null;
            }

            var seAudioInfo = Create(seName, AudioType.SE, clip, duration <= 0 ? volume : 0);
            seAudioInfos.Add(seAudioInfo);
            seAudioInfo.Source.loop = loop;
            seAudioInfo.Source.Play();

            return duration <= 0 ? null : Fade(seAudioInfo, duration, 0, volume, null);
        }

        public void StopSE(string seName)
        {
            var seAudioInfo = FindSE(seName);
            if (seAudioInfo == null) return;

            seAudioInfo.Source.Stop();
            Destroy(seAudioInfo);
        }

        public IEnumerator StopSE(string seName, float fadeTime)
        {
            var seAudioInfo = FindSE(seName);
            if (seAudioInfo == null) return null;

            if (fadeTime > 0)
            {
                return Fade(seAudioInfo, fadeTime, seAudioInfo.Volume, 0, () => Destroy(seAudioInfo));
            }

            seAudioInfo.Source.Stop();
            Destroy(seAudioInfo);
            return null;
        }

        private AudioInfo FindSE(string seName)
        {
            return seAudioInfos.Exists(info => info.Name == seName) ? seAudioInfos.Find(info => info.Name == seName) : null;
        }

        public static bool IsInvalidVoice(string voiceName)
        {
            return voiceName.StartsWith("null") || string.IsNullOrWhiteSpace(voiceName);
        }

        private AudioInfo Create(string audioName, AudioType type, AudioClip audioClip, float volume)
        {
            var newObj = new GameObject(audioName);
            newObj.transform.SetParent(transform);
            var newSource = newObj.AddComponent<AudioSource>();
            newSource.clip = audioClip;
            newSource.spatialize = false;
            newSource.spatialBlend = 0;
            AudioInfo info = new()
            {
                Name = audioName,
                Source = newSource,
            };
            switch (type)
            {
                case AudioType.BGM:
                    newSource.outputAudioMixerGroup = bgmMixerGroup;
                    newSource.loop = true;
                    info.GetGameSettingsVolume = () => GameSettings.AudioBGMVolume;
                    break;
                case AudioType.SE:
                    newSource.outputAudioMixerGroup = seMixerGroup;
                    info.GetGameSettingsVolume = () => GameSettings.AudioSEVolume;
                    break;
                case AudioType.Voice:
                    newSource.outputAudioMixerGroup = voiceMixerGroup;
                    info.GetGameSettingsVolume = () => GameSettings.AudioVoiceVolume;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            info.Volume = volume;
            return info;
        }

        private void Destroy(AudioInfo info)
        {
            if (info == null) return;
            if (info.Source == null) return;
            Destroy(info.Source.gameObject);
        }

        private bool IsDead(AudioInfo info)
        {
            if (info == null) return true;
            if (info.Source == null) return true;
            if (info.Source.clip == null) return true;
            if (info.Source.loop) return false; // looped audio never dies
            if (info.Source.isPlaying) return false; // if the audio is playing, it's not dead
            if (running) return true; // if the game is running, the audio should be dead
            return info.Source.time >= info.Source.clip.length; // if the game is not running, the audio should be dead when it reaches the end
        }

        private IEnumerator Fade(AudioInfo info, float time, float fromVolume, float toVolume, Action callback)
        {
            if (time <= 0)
            {
                info.Volume = toVolume;
                yield break;
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(DOVirtual.Float(fromVolume, toVolume, time, value => info.Volume = value));
            seq.OnComplete(() => callback?.Invoke());

            yield return seq.WaitForCompletion();
        }

        public class AudioInfo
        {
            public string Name { get; set; }
            public AudioSource Source { get; set; }

            private float volume = 1.0f;
            public float Volume
            {
                get => volume;
                set
                {
                    volume = value;
                    if (Source != null)
                    {
                        Source.volume = GameSettings.AudioMasterVolume * GetGameSettingsVolume() * volume;
                    }
                }
            }
            public Func<float> GetGameSettingsVolume { get; set; } = () => 1.0f;
        }

        private enum AudioType
        {
            BGM,
            SE,
            Voice
        }

        public AudioState GetState()
        {
            return new()
            {
                bgmPlaying = bgmAudioInfo != null && bgmAudioInfo.Source != null && bgmAudioInfo.Source.isPlaying,
                bgmName = bgmAudioInfo == null || bgmAudioInfo.Source == null ? null : bgmAudioInfo.Name,
                bgmVolume = bgmAudioInfo == null || bgmAudioInfo.Source == null ? 0 : bgmAudioInfo.Volume
            };
        }

        public void RestoreState(AudioState state)
        {
            if (state.bgmPlaying)
            {
                if (bgmAudioInfo != null && bgmAudioInfo.Source != null)
                {
                    if (state.bgmName != bgmAudioInfo.Name)
                    {
                        PlayBGM(state.bgmName, state.bgmVolume);
                    }
                    else
                    {
                        bgmAudioInfo.Volume = state.bgmVolume;
                    }
                }
            }
            else
            {
                StopAudioImmediately(bgmAudioInfo);
                bgmAudioInfo = null;
            }

            StopVoice();
            foreach (var se in seAudioInfos)
            {
                StopSE(se.Name);
            }
        }
    }
}