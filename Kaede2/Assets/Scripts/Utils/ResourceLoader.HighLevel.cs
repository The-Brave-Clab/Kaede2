using Kaede2.Assets.ScriptableObjects;
using UnityEngine;

namespace Kaede2.Utils
{
    public partial class ResourceLoader
    {
        public Request<AudioLoopInfo> LoadAudioLoopInfo(Request<AudioClip> audioRequest)
        {
            return Load<AudioLoopInfo>(audioRequest.Path.Replace(".wav", ".loopinfo"));
        }

        // audio

        public Request<AudioClip> LoadSystemBackgroundMusic(string bgmName)
        {
            return Load<AudioClip>($"audio/bgm/{bgmName}.wav");
        }

        public Request<AudioClip> LoadCharacterVoice(string voiceName)
        {
            return Load<AudioClip>($"audio/character_voice/{voiceName}.wav");
        }

        public Request<AudioClip> LoadSystemSoundEffect(string seName)
        {
            return Load<AudioClip>($"audio/system_se/{seName}.wav");
        }

        public Request<AudioClip> LoadSystemVoice(string voiceName)
        {
            return Load<AudioClip>($"audio/system_voice/{voiceName}.wav");
        }

        // illust

        public Request<Sprite> LoadIllustration(string illustName)
        {
            return Load<Sprite>($"illust/{illustName}.png");
        }

        // master_data

        public Request<T> LoadMasterData<T>() where T : BaseMasterData
        {
            return Load<T>($"master_data/{typeof(T).Name.ToLower()}.masterdata");
        }

        // scenario_common

        public Request<Sprite> LoadScenarioBackground(string bgName)
        {
            return Load<Sprite>($"scenario_common/bg/{bgName}.png");
        }

        public Request<Sprite> LoadScenarioSprite(string spriteName)
        {
            return Load<Sprite>($"scenario_common/sprite/{spriteName}.png");
        }

        public Request<Sprite> LoadScenarioCharacterIcon(string charaIcon)
        {
            return Load<Sprite>($"scenario_common/icon/{charaIcon}.png");
        }

        public Request<AudioClip> LoadScenarioBackgroundMusic(string bgmName)
        {
            return Load<AudioClip>($"scenario_common/bgm/{bgmName}.wav");
        }

        public Request<AudioClip> LoadScenarioSoundEffect(string seName)
        {
            return Load<AudioClip>($"scenario_common/se/{seName}.wav");
        }

        public Request<AudioClip> LoadScenarioJingle(string jingleName)
        {
            return Load<AudioClip>($"scenario_common/jingle/{jingleName}.wav");
        }

        public Request<TextAsset> LoadScenarioDefineText(string defineTextName)
        {
            return Load<TextAsset>($"scenario_common/define/{defineTextName}.txt");
        }

        public Live2DLoadRequest LoadLive2DModel(string modelName)
        {
            return new Live2DLoadRequest(modelName, this);
        }

        // scenario

        public Request<TextAsset> LoadScenarioScriptText(string scenario)
        {
            return Load<TextAsset>($"scenario/{scenario}/{scenario}_script.txt");
        }

        public Request<TextAsset> LoadScenarioAliasText(string scenario)
        {
            return Load<TextAsset>($"scenario/{scenario}/{scenario}_alias.txt");
        }

        public Request<TextAsset> LoadScenarioIgnoreText(string scenario)
        {
            return Load<TextAsset>($"scenario/{scenario}/{scenario}_ignore.txt");
        }

        public Request<AudioClip> LoadScenarioVoice(string scenario, string voiceName)
        {
            return Load<AudioClip>($"scenario/{scenario}/voice/{voiceName}.wav");
        }

        public Request<Sprite> LoadScenarioStill(string scenario, string stillImage)
        {
            return Load<Sprite>($"scenario/{scenario}/still/{stillImage}.png");
        }
    }
}