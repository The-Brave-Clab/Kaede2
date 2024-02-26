using Kaede2.ScriptableObjects;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Kaede2.Utils
{
    public static partial class ResourceLoader
    {
        public static AsyncOperationHandle<T> Load<T>(string assetAddress) where T : Object
        {
            assetAddress = assetAddress.ToLower();
            return Addressables.LoadAssetAsync<T>(assetAddress);
        }

        public static AsyncOperationHandle<AudioLoopInfo> LoadAudioLoopInfo(string audioAssetAddress)
        {
            return LoadAudioLoopInfo(audioAssetAddress, out _);
        }

        public static AsyncOperationHandle<AudioLoopInfo> LoadAudioLoopInfo(string audioAssetAddress, out string assetAddress)
        {
            assetAddress = audioAssetAddress;
            return Load<AudioLoopInfo>(audioAssetAddress);
        }

        // audio

        public static AsyncOperationHandle<AudioClip> LoadSystemBackgroundMusic(string bgmName)
        {
            return LoadSystemBackgroundMusic(bgmName, out _);
        }

        public static AsyncOperationHandle<AudioClip> LoadSystemBackgroundMusic(string bgmName, out string assetAddress)
        {
            assetAddress = $"audio/bgm/{bgmName}.wav";
            return Load<AudioClip>(assetAddress);
        }

        public static AsyncOperationHandle<AudioClip> LoadCharacterVoice(string voiceName)
        {
            return LoadCharacterVoice(voiceName, out _);
        }

        public static AsyncOperationHandle<AudioClip> LoadCharacterVoice(string voiceName, out string assetAddress)
        {
            assetAddress = $"audio/character_voice/{voiceName}.wav";
            return Load<AudioClip>(assetAddress);
        }

        public static AsyncOperationHandle<AudioClip> LoadSystemSoundEffect(string seName)
        {
            return LoadSystemSoundEffect(seName, out _);
        }

        public static AsyncOperationHandle<AudioClip> LoadSystemSoundEffect(string seName, out string assetAddress)
        {
            assetAddress = $"audio/system_se/{seName}.wav";
            return Load<AudioClip>(assetAddress);
        }

        public static AsyncOperationHandle<AudioClip> LoadSystemVoice(string voiceName)
        {
            return LoadSystemVoice(voiceName, out _);
        }

        public static AsyncOperationHandle<AudioClip> LoadSystemVoice(string voiceName, out string assetAddress)
        {
            assetAddress = $"audio/system_voice/{voiceName}.wav";
            return Load<AudioClip>(assetAddress);
        }

        // illust

        public static AsyncOperationHandle<Sprite> LoadIllustration(string illustName)
        {
            return LoadIllustration(illustName, out _);
        }

        public static AsyncOperationHandle<Sprite> LoadIllustration(string illustName, out string assetAddress)
        {
            assetAddress = $"illust/{illustName}.png";
            return Load<Sprite>(assetAddress);
        }

        // master_data

        public static AsyncOperationHandle<T> LoadMasterData<T>() where T : BaseMasterData
        {
            return LoadMasterData<T>(out _);
        }

        public static AsyncOperationHandle<T> LoadMasterData<T>(out string assetAddress) where T : BaseMasterData
        {
            assetAddress = $"master_data/{typeof(T).Name}.masterdata";
            return Load<T>(assetAddress);
        }

        // scenario_common

        public static AsyncOperationHandle<Sprite> LoadScenarioBackground(string bgName)
        {
            return LoadScenarioBackground(bgName, out _);
        }

        public static AsyncOperationHandle<Sprite> LoadScenarioBackground(string bgName, out string assetAddress)
        {
            assetAddress = $"scenario_common/bg/{bgName}.png";
            return Load<Sprite>(assetAddress);
        }

        public static AsyncOperationHandle<Sprite> LoadScenarioSprite(string spriteName)
        {
            return LoadScenarioSprite(spriteName, out _);
        }

        public static AsyncOperationHandle<Sprite> LoadScenarioSprite(string spriteName, out string assetAddress)
        {
            assetAddress = $"scenario_common/sprite/{spriteName}.png";
            return Load<Sprite>(assetAddress);
        }

        public static AsyncOperationHandle<Sprite> LoadScenarioCharacterIcon(string charaIcon)
        {
            return LoadScenarioCharacterIcon(charaIcon, out _);
        }
        
        public static AsyncOperationHandle<Sprite> LoadScenarioCharacterIcon(string charaIcon, out string assetAddress)
        {
            assetAddress = $"scenario_common/icon/{charaIcon}.png";
            return Load<Sprite>(assetAddress);
        }

        public static AsyncOperationHandle<AudioClip> LoadScenarioBackgroundMusic(string bgmName)
        {
            return LoadScenarioBackgroundMusic(bgmName, out _);
        }

        public static AsyncOperationHandle<AudioClip> LoadScenarioBackgroundMusic(string bgmName, out string assetAddress)
        {
            assetAddress = $"scenario_common/bgm/{bgmName}.wav";
            return Load<AudioClip>(assetAddress);
        }

        public static AsyncOperationHandle<AudioClip> LoadScenarioSoundEffect(string seName)
        {
            return LoadScenarioSoundEffect(seName, out _);
        }

        public static AsyncOperationHandle<AudioClip> LoadScenarioSoundEffect(string seName, out string assetAddress)
        {
            assetAddress = $"scenario_common/se/{seName}.wav";
            return Load<AudioClip>(assetAddress);
        }

        public static AsyncOperationHandle<AudioClip> LoadScenarioJingle(string jingleName)
        {
            return LoadScenarioJingle(jingleName, out _);
        }

        public static AsyncOperationHandle<AudioClip> LoadScenarioJingle(string jingleName, out string assetAddress)
        {
            assetAddress = $"scenario_common/jingle/{jingleName}.wav";
            return Load<AudioClip>(assetAddress);
        }

        public static AsyncOperationHandle<TextAsset> LoadScenarioDefineText(string defineTextName)
        {
            return LoadScenarioDefineText(defineTextName, out _);
        }

        public static AsyncOperationHandle<TextAsset> LoadScenarioDefineText(string defineTextName, out string assetAddress)
        {
            assetAddress = $"scenario_common/define/{defineTextName}.txt";
            return Load<TextAsset>(assetAddress);
        }

        public static Live2DLoadRequest LoadLive2DModel(string modelName)
        {
            return new Live2DLoadRequest(modelName);
        }

        // scenario

        public static AsyncOperationHandle<TextAsset> LoadScenarioScriptText(string scenario)
        {
            return LoadScenarioScriptText(scenario, out _);
        }

        public static AsyncOperationHandle<TextAsset> LoadScenarioScriptText(string scenario, out string assetAddress)
        {
            assetAddress = $"scenario/{scenario}/{scenario}_script.txt";
            return Load<TextAsset>(assetAddress);
        }

        public static AsyncOperationHandle<TextAsset> LoadScenarioAliasText(string scenario)
        {
            return LoadScenarioAliasText(scenario, out _);
        }

        public static AsyncOperationHandle<TextAsset> LoadScenarioAliasText(string scenario, out string assetAddress)
        {
            assetAddress = $"scenario/{scenario}/{scenario}_alias.txt";
            return Load<TextAsset>(assetAddress);
        }

        public static AsyncOperationHandle<TextAsset> LoadScenarioIgnoreText(string scenario)
        {
            return LoadScenarioIgnoreText(scenario, out _);
        }

        public static AsyncOperationHandle<TextAsset> LoadScenarioIgnoreText(string scenario, out string assetAddress)
        {
            assetAddress = $"scenario/{scenario}/{scenario}_ignore.txt";
            return Load<TextAsset>(assetAddress);
        }

        public static AsyncOperationHandle<AudioClip> LoadScenarioVoice(string scenario, string voiceName)
        {
            return LoadScenarioVoice(scenario, voiceName, out _);
        }

        public static AsyncOperationHandle<AudioClip> LoadScenarioVoice(string scenario, string voiceName, out string assetAddress)
        {
            assetAddress = $"scenario/{scenario}/voice/{voiceName}.wav";
            return Load<AudioClip>(assetAddress);
        }

        public static AsyncOperationHandle<Sprite> LoadScenarioStill(string scenario, string stillImage)
        {
            return LoadScenarioStill(scenario, stillImage, out _);
        }

        public static AsyncOperationHandle<Sprite> LoadScenarioStill(string scenario, string stillImage, out string assetAddress)
        {
            assetAddress = $"scenario/{scenario}/still/{stillImage}.png";
            return Load<Sprite>(assetAddress);
        }
    }
}