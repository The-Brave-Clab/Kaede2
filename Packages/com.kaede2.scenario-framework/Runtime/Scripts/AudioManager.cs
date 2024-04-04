using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

namespace Kaede2.Scenario.Framework
{
    public class AudioManager : MonoBehaviour, IStateSavable<AudioState>
    {
        public ScenarioModule Module { get; set; }

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

        private const int FFTSize = 128;

        protected void Awake()
        {
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
                else
                {
                    bgmAudioInfo.UpdateVolume();
                }
            }

            if (voiceAudioInfo != null)
            {
                if (IsDead(voiceAudioInfo))
                {
                    Destroy(voiceAudioInfo);
                    voiceAudioInfo = null;
                }
                else
                {
                    voiceAudioInfo.UpdateVolume();
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
                else
                {
                    seAudioInfo.UpdateVolume();
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

            if (!Module.ScenarioResource.BackgroundMusics.TryGetValue(bgmName, out var clip))
            {
                Debug.LogError($"BGM {bgmName} not found");
                if (ScenarioRunMode.Args.TestMode)
                    ScenarioRunMode.FailTest(ScenarioRunMode.FailReason.ResourceNotFound);
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

            if (!Module.ScenarioResource.Voices.TryGetValue(voiceName, out var clip))
            {
                Debug.LogError($"Voice {voiceName} not found");
                if (ScenarioRunMode.Args.TestMode)
                    ScenarioRunMode.FailTest(ScenarioRunMode.FailReason.ResourceNotFound);
                return;
            }

            voiceAudioInfo = Create(voiceName, AudioType.Voice, clip, 1.0f);
            voiceAudioInfo.Source.Play();

#if UNITY_WEBGL && !UNITY_EDITOR
            WaitUntilContidionAndRun(() =>
            {
                StartAudioSampling(voiceAudioInfo.UniqueName, clip.samples, FFTSize);
                voiceAudioInfo.CanGetSpectrumData = true;
            }, () => clip.samples > 0);
#endif
        }

        public void StopVoice()
        {
            StopAudioImmediately(voiceAudioInfo);
#if UNITY_WEBGL && !UNITY_EDITOR
            CloseAudioSampling(voiceAudioInfo.UniqueName);
#endif
            voiceAudioInfo = null;
        }

        public float GetVoiceVolume()
        {
            if (voiceAudioInfo == null) return 0.0f;
            if (voiceAudioInfo.Source == null) return 0.0f;
            if (!voiceAudioInfo.Source.isPlaying) return 0.0f;
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!voiceAudioInfo.CanGetSpectrumData) return 0.0f;
#endif
            float[] data = new float[FFTSize];
#if UNITY_WEBGL && !UNITY_EDITOR
            GetAudioSamples(voiceAudioInfo.UniqueName, data, FFTSize);
#else
            voiceAudioInfo.Source.GetSpectrumData(data, 0, FFTWindow.Rectangular);
#endif
            return data.Sum(Mathf.Abs);
        }

        public bool IsVoicePlaying()
        {
            return !IsDead(voiceAudioInfo);
        }

        public IEnumerator PlaySE(string seName, float volume, float duration, bool loop)
        {
            if (!Module.ScenarioResource.SoundEffects.TryGetValue(seName, out var clip))
            {
                Debug.LogError($"SE {seName} not found");
                if (ScenarioRunMode.Args.TestMode)
                    ScenarioRunMode.FailTest(ScenarioRunMode.FailReason.ResourceNotFound);
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

            float GetMasterVolume()
            {
                return Module.AudioMasterVolume;
            }

            switch (type)
            {
                case AudioType.BGM:
                    newSource.outputAudioMixerGroup = bgmMixerGroup;
                    newSource.loop = true;
                    info.GetGameSettingsMasterVolume = GetMasterVolume;
                    info.GetGameSettingsVolume = () => Module.AudioBGMVolume;
                    break;
                case AudioType.SE:
                    newSource.outputAudioMixerGroup = seMixerGroup;
                    info.GetGameSettingsMasterVolume = GetMasterVolume;
                    info.GetGameSettingsVolume = () => Module.AudioSEVolume;
                    break;
                case AudioType.Voice:
                    newSource.outputAudioMixerGroup = voiceMixerGroup;
                    info.GetGameSettingsMasterVolume = GetMasterVolume;
                    info.GetGameSettingsVolume = () => Module.AudioVoiceVolume;
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

        private class AudioInfo
        {
            public string Name { get; set; }
            public AudioSource Source { get; set; }
#if UNITY_WEBGL && !UNITY_EDITOR
            public bool CanGetSpectrumData { get; set; } = false;
            private Guid ID { get; set; } = Guid.NewGuid();

            public string UniqueName => $"{Name} ({ID})";
#endif

            private float volume = 1.0f;
            public float Volume
            {
                get => volume;
                set
                {
                    volume = value;
                    UpdateVolume();
                }
            }
            public Func<float> GetGameSettingsMasterVolume { get; set; } = () => 1.0f;
            public Func<float> GetGameSettingsVolume { get; set; } = () => 1.0f;

            public void UpdateVolume()
            {
                if (Source == null) return;
                Source.volume = ScenarioRunMode.Args.BatchMode ? 0 : GetGameSettingsMasterVolume() * GetGameSettingsVolume() * volume;
            }
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

#if UNITY_WEBGL && !UNITY_EDITOR
        private const string WebDllName = "__Internal";

        [DllImport(WebDllName)]
        private static extern bool StartAudioSampling(string name, float samples, int bufferSize);

        [DllImport(WebDllName)]
        private static extern bool CloseAudioSampling(string name);

        [DllImport(WebDllName)]
        private static extern bool GetAudioSamples(string name, float[] freqData, int size);

        private void WaitUntilContidionAndRun(Action action, Func<bool> condition)
        {
            IEnumerator WaitUntilCondition()
            {
                yield return new WaitUntil(condition);
                action();
            }

            StartCoroutine(WaitUntilCondition());
        }
#endif
    }
}