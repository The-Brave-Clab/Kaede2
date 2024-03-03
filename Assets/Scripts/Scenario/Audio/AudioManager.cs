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

        protected override void Awake()
        {
            base.Awake();

            bgmAudioInfo = null;
            voiceAudioInfo = null;
            seAudioInfos = new();
        }

        private void Update()
        {
            ClearDeadAudio();
            UpdateAudioVolume();
        }

        public void PlayBGM(string bgmName, float volume)
        {
            StopBGM();

            if (!ScenarioModule.Instance.ScenarioResource.backgroundMusics.TryGetValue(bgmName, out var clip))
            {
                Debug.LogError($"BGM {bgmName} not found");
                return;
            }
            bgmAudioInfo = Create(bgmName, AudioType.BGM, clip, volume);
            UpdateAudioVolume();
            bgmAudioInfo.source.Play();
        }

        public void StopBGM()
        {
            if (bgmAudioInfo == null) return;
            if (bgmAudioInfo.source == null) return;
            bgmAudioInfo.source.Stop();
            Destroy(bgmAudioInfo);
            bgmAudioInfo = null;
        }

        public IEnumerator StopBGM(float fadeTime)
        {
            if (fadeTime > 0)
            {
                if (bgmAudioInfo == null) return null;
                if (bgmAudioInfo.source == null) return null;
                return Fade(bgmAudioInfo, fadeTime, bgmAudioInfo.volume, 0, StopBGM);
            }

            StopBGM();
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
            UpdateAudioVolume();
            voiceAudioInfo.source.Play();
        }

        public void StopVoice()
        {
            if (voiceAudioInfo == null) return;
            if (voiceAudioInfo.source == null) return;
            voiceAudioInfo.source.Stop();
            Destroy(voiceAudioInfo);
            voiceAudioInfo = null;
        }

        public float GetVoiceVolume()
        {
            if (voiceAudioInfo == null) return 0.0f;
            if (voiceAudioInfo.source == null) return 0.0f;
            if (!voiceAudioInfo.source.isPlaying) return 0.0f;
            return GetCurrentVolume(voiceAudioInfo.source);
        }

        public bool IsVoicePlaying()
        {
            return voiceAudioInfo != null && voiceAudioInfo.source != null && voiceAudioInfo.source.isPlaying;
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
            seAudioInfo.source.loop = loop;
            UpdateAudioVolume();
            seAudioInfo.source.Play();

            return duration <= 0 ? null : Fade(seAudioInfo, duration, 0, volume, null);
        }

        public void StopSE(string seName)
        {
            var seAudioInfo = FindSE(seName);
            if (seAudioInfo == null) return;

            seAudioInfo.source.Stop();
            Destroy(seAudioInfo);
        }

        public IEnumerator StopSE(string seName, float fadeTime)
        {
            var seAudioInfo = FindSE(seName);
            if (seAudioInfo == null) return null;

            if (fadeTime > 0)
            {
                return Fade(seAudioInfo, fadeTime, seAudioInfo.volume, 0, () => Destroy(seAudioInfo));
            }

            seAudioInfo.source.Stop();
            Destroy(seAudioInfo);
            return null;
        }

        private AudioInfo FindSE(string seName)
        {
            return seAudioInfos.Exists(info => info.name == seName) ? seAudioInfos.Find(info => info.name == seName) : null;
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
            switch (type)
            {
                case AudioType.BGM:
                    newSource.outputAudioMixerGroup = bgmMixerGroup;
                    newSource.loop = true;
                    break;
                case AudioType.SE:
                    newSource.outputAudioMixerGroup = seMixerGroup;
                    break;
                case AudioType.Voice:
                    newSource.outputAudioMixerGroup = voiceMixerGroup;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return new()
            {
                source = newSource,
                volume = volume
            };
        }

        private void Destroy(AudioInfo info)
        {
            if (info == null) return;
            if (info.source == null) return;
            Destroy(info.source.gameObject);
        }

        private void UpdateAudioVolume()
        {
            if (bgmAudioInfo != null && bgmAudioInfo.source != null)
                bgmAudioInfo.source.volume = GameSettings.AudioMasterVolume * GameSettings.AudioBGMVolume * bgmAudioInfo.volume;

            if (voiceAudioInfo != null && voiceAudioInfo.source != null)
                voiceAudioInfo.source.volume = GameSettings.AudioMasterVolume * GameSettings.AudioVoiceVolume * voiceAudioInfo.volume;

            foreach (var seAudioInfo in seAudioInfos)
                seAudioInfo.source.volume = GameSettings.AudioMasterVolume * GameSettings.AudioSEVolume * seAudioInfo.volume;
        }

        private bool IsDead(AudioSource source)
        {
            if (source == null) return true;
            if (source.clip == null) return true;
            if (source.time >= source.clip.length) return true;
            return false;
        }

        private void ClearDeadAudio()
        {
            if (bgmAudioInfo != null)
            {
                if (IsDead(bgmAudioInfo.source))
                {
                    Destroy(bgmAudioInfo);
                    bgmAudioInfo = null;
                }
            }

            if (voiceAudioInfo != null)
            {
                if (IsDead(voiceAudioInfo.source))
                {
                    Destroy(voiceAudioInfo);
                    voiceAudioInfo = null;
                }
            }

            List<AudioInfo> toBeRemoved = new();
            foreach (var seAudioInfo in seAudioInfos)
            {
                if (IsDead(seAudioInfo.source))
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

        private IEnumerator Fade(AudioInfo info, float time, float fromVolume, float toVolume, Action callback)
        {
            if (time <= 0)
            {
                info.volume = toVolume;
                yield break;
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(DOVirtual.Float(fromVolume, toVolume, time, value => info.volume = value));
            seq.OnComplete(() => callback?.Invoke());

            yield return seq.WaitForCompletion();
        }

        public class AudioInfo
        {
            public string name;
            public AudioSource source;
            public float volume = 1.0f;
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
                bgmPlaying = bgmAudioInfo != null && bgmAudioInfo.source != null && bgmAudioInfo.source.isPlaying,
                bgmName = bgmAudioInfo == null || bgmAudioInfo.source == null ? null : bgmAudioInfo.name,
                bgmVolume = bgmAudioInfo == null || bgmAudioInfo.source == null ? 0 : bgmAudioInfo.volume
            };
        }

        public void RestoreState(AudioState state)
        {
            if (state.bgmPlaying)
            {
                if (bgmAudioInfo != null && bgmAudioInfo.source != null)
                {
                    if (state.bgmName != bgmAudioInfo.name)
                    {
                        PlayBGM(state.bgmName, state.bgmVolume);
                    }
                    else
                    {
                        bgmAudioInfo.volume = state.bgmVolume;
                    }
                }
            }
            else
            {
                StopBGM();
            }

            StopVoice();
            foreach (var se in seAudioInfos)
            {
                StopSE(se.name);
            }
        }
    }
}