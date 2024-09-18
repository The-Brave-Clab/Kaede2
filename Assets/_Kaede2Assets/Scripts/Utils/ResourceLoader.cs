using Kaede2.Scenario.Framework;
using Kaede2.Scenario.Framework.Live2D;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Kaede2.Utils
{
    public static partial class ResourceLoader
    {
        // audio

        public static AsyncOperationHandle<AudioClip> LoadSystemBackgroundMusic(string bgmName)
        {
            return Addressables.LoadAssetAsync<AudioClip>($"audio/bgm/{bgmName}.wav");
        }

        public static AsyncOperationHandle<AudioClip> LoadCharacterVoice(string voiceName)
        {
            return Addressables.LoadAssetAsync<AudioClip>($"audio/character_voice/{voiceName}.wav");
        }

        public static AsyncOperationHandle<AudioClip> LoadSystemSoundEffect(string seName)
        {
            return Addressables.LoadAssetAsync<AudioClip>($"audio/system_se/{seName}.wav");
        }

        public static AsyncOperationHandle<AudioClip> LoadSystemVoice(string voiceName)
        {
            return Addressables.LoadAssetAsync<AudioClip>($"audio/system_voice/{voiceName}.wav");
        }

        // illust

        public static AsyncOperationHandle<Sprite> LoadIllustration(string illustName, bool thumbnail = false)
        {
            var subfolder = thumbnail ? "thumbnail" : "original";
            return Addressables.LoadAssetAsync<Sprite>($"illust/{subfolder}/{illustName}.png");
        }

        // cartoon

        public static AsyncOperationHandle<Sprite> LoadCartoonFrame(string frameName)
        {
            return Addressables.LoadAssetAsync<Sprite>($"cartoon_images/frame/{frameName}.png");
        }

        // scenario_common

        public static AsyncOperationHandle<Texture2D> LoadScenarioBackground(string bgName)
        {
            return Addressables.LoadAssetAsync<Texture2D>($"scenario_common/bg/{bgName}.png");
        }

        public static AsyncOperationHandle<Sprite> LoadScenarioSprite(string spriteName)
        {
            return Addressables.LoadAssetAsync<Sprite>($"scenario_common/sprite/{spriteName}.png");
        }
        
        public static AsyncOperationHandle<Sprite> LoadScenarioCharacterIcon(string charaIcon)
        {
            return Addressables.LoadAssetAsync<Sprite>($"scenario_common/icon/{charaIcon}.png");
        }

        public static AsyncOperationHandle<AudioClip> LoadScenarioBackgroundMusic(string bgmName)
        {
            return Addressables.LoadAssetAsync<AudioClip>($"scenario_common/bgm/{bgmName}.wav");
        }

        public static AsyncOperationHandle<AudioClip> LoadScenarioSoundEffect(string seName)
        {
            return Addressables.LoadAssetAsync<AudioClip>($"scenario_common/se/{seName}.wav");
        }

        public static AsyncOperationHandle<AudioClip> LoadScenarioJingle(string jingleName)
        {
            return Addressables.LoadAssetAsync<AudioClip>($"scenario_common/jingle/{jingleName}.wav");
        }

        public static AsyncOperationHandle<TextAsset> LoadScenarioDefineText(string defineTextName)
        {
            return Addressables.LoadAssetAsync<TextAsset>($"scenario_common/define/{defineTextName}.txt");
        }

        public static AsyncOperationHandle<Live2DAssets> LoadLive2DModel(string modelName)
        {
            var loadOperation = new Live2DOperation(modelName);
            return Addressables.ResourceManager.StartOperation(loadOperation, default);
        }

        public static AsyncOperationHandle<Sprite> LoadScenarioTransformEffectSprite(CharacterId characterId)
        {
            return Addressables.LoadAssetAsync<Sprite>($"scenario_common/trans_scene/{(int)characterId:00}.png");
        }

        // scenario

        public static AsyncOperationHandle<TextAsset> LoadScenarioScriptText(string scenario)
        {
            return Addressables.LoadAssetAsync<TextAsset>($"scenario/{scenario}/{scenario}_script.txt");
        }

        public static AsyncOperationHandle<TextAsset> LoadScenarioAliasText(string scenario, string aliasFileName)
        {
            return Addressables.LoadAssetAsync<TextAsset>($"scenario/{scenario}/{aliasFileName}.txt");
        }

        public static AsyncOperationHandle<TextAsset> LoadScenarioIgnoreText(string scenario)
        {
            return Addressables.LoadAssetAsync<TextAsset>($"scenario/{scenario}/{scenario}_ignore.txt");
        }

        public static AsyncOperationHandle<AudioClip> LoadScenarioVoice(string scenario, string voiceName)
        {
            return Addressables.LoadAssetAsync<AudioClip>($"scenario/{scenario}/voice/{voiceName}.wav");
        }

        public static AsyncOperationHandle<Texture2D> LoadScenarioStill(string scenario, string stillImage)
        {
            return Addressables.LoadAssetAsync<Texture2D>($"scenario/{scenario}/still/{stillImage}.png");
        }
    }
}