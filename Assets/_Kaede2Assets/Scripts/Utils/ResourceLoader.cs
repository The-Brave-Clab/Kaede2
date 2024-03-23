using System.IO;
using Kaede2.Scenario.Framework;
using UnityEngine;
using Kaede2.ScriptableObjects;

namespace Kaede2.Utils
{
    public static partial class ResourceLoader
    {
        public static LoadAddressableHandle<T> Load<T>(string assetAddress) where T : Object
        {
            return new LoadAddressableHandle<T>(assetAddress);
        }

        public static LoadAddressableHandle<AudioLoopInfo> LoadAudioLoopInfo(LoadAddressableHandle<AudioClip> audioLoadHandle)
        {
            return Load<AudioLoopInfo>(Path.ChangeExtension(audioLoadHandle.AssetAddress, ".loopinfo"));
        }

        // audio

        public static LoadAddressableHandle<AudioClip> LoadSystemBackgroundMusic(string bgmName)
        {
            return Load<AudioClip>($"audio/bgm/{bgmName}.wav");
        }

        public static LoadAddressableHandle<AudioClip> LoadCharacterVoice(string voiceName)
        {
            return Load<AudioClip>($"audio/character_voice/{voiceName}.wav");
        }

        public static LoadAddressableHandle<AudioClip> LoadSystemSoundEffect(string seName)
        {
            return Load<AudioClip>($"audio/system_se/{seName}.wav");
        }

        public static LoadAddressableHandle<AudioClip> LoadSystemVoice(string voiceName)
        {
            return Load<AudioClip>($"audio/system_voice/{voiceName}.wav");
        }

        // illust

        public static LoadAddressableHandle<Sprite> LoadIllustration(string illustName)
        {
            return Load<Sprite>($"illust/{illustName}.png");
        }

        // master_data

        public static LoadAddressableHandle<T> LoadMasterData<T>() where T : BaseMasterData
        {
            return Load<T>($"master_data/{typeof(T).Name}.masterdata");
        }

        // scenario_common

        public static LoadAddressableHandle<Texture2D> LoadScenarioBackground(string bgName)
        {
            return Load<Texture2D>($"scenario_common/bg/{bgName}.png");
        }

        public static LoadAddressableHandle<Sprite> LoadScenarioSprite(string spriteName)
        {
            return Load<Sprite>($"scenario_common/sprite/{spriteName}.png");
        }
        
        public static LoadAddressableHandle<Sprite> LoadScenarioCharacterIcon(string charaIcon)
        {
            return Load<Sprite>($"scenario_common/icon/{charaIcon}.png");
        }

        public static LoadAddressableHandle<AudioClip> LoadScenarioBackgroundMusic(string bgmName)
        {
            return Load<AudioClip>($"scenario_common/bgm/{bgmName}.wav");
        }

        public static LoadAddressableHandle<AudioClip> LoadScenarioSoundEffect(string seName)
        {
            return Load<AudioClip>($"scenario_common/se/{seName}.wav");
        }

        public static LoadAddressableHandle<AudioClip> LoadScenarioJingle(string jingleName)
        {
            return Load<AudioClip>($"scenario_common/jingle/{jingleName}.wav");
        }

        public static LoadAddressableHandle<TextAsset> LoadScenarioDefineText(string defineTextName)
        {
            return Load<TextAsset>($"scenario_common/define/{defineTextName}.txt");
        }

        public static LoadLive2DHandle LoadLive2DModel(string modelName)
        {
            return new LoadLive2DHandle(modelName);
        }

        public static LoadAddressableHandle<Sprite> LoadScenarioTransformEffectSprite(CharacterId characterId)
        {
            return Load<Sprite>($"scenario_common/trans_scene/{(int)characterId:00}.png");
        }

        // scenario

        public static LoadAddressableHandle<TextAsset> LoadScenarioScriptText(string scenario)
        {
            return Load<TextAsset>($"scenario/{scenario}/{scenario}_script.txt");
        }

        public static LoadAddressableHandle<TextAsset> LoadScenarioAliasText(string scenario, string aliasFileName)
        {
            return Load<TextAsset>($"scenario/{scenario}/{aliasFileName}.txt");
        }

        public static LoadAddressableHandle<TextAsset> LoadScenarioIgnoreText(string scenario)
        {
            return Load<TextAsset>($"scenario/{scenario}/{scenario}_ignore.txt");
        }

        public static LoadAddressableHandle<AudioClip> LoadScenarioVoice(string scenario, string voiceName)
        {
            return Load<AudioClip>($"scenario/{scenario}/voice/{voiceName}.wav");
        }

        public static LoadAddressableHandle<Texture2D> LoadScenarioStill(string scenario, string stillImage)
        {
            return Load<Texture2D>($"scenario/{scenario}/still/{stillImage}.png");
        }
    }
}