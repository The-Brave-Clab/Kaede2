using System.Collections.Generic;
using UnityEngine;
using Kaede2.Live2D;
using Kaede2.ScriptableObjects;
using Kaede2.Utils;

namespace Kaede2.Scenario
{
    public partial class ScenarioModule
    {
        public class Resource
        {
            public TextAsset aliasText = null;
            public Dictionary<string, Live2DAssets> actors = new();
            public Dictionary<string, Sprite> sprites = new();
            public Dictionary<string, Texture2D> stills = new();
            public Dictionary<string, Texture2D> backgrounds = new();
            public Dictionary<string, AudioClip> soundEffects = new();
            public Dictionary<string, AudioClip> backgroundMusics = new();
            public Dictionary<string, AudioClip> voices = new();
            public Dictionary<CharacterId, Sprite> transformImages = new();
        }

        private Resource scenarioResource;

        public Resource ScenarioResource => scenarioResource;

        public void RegisterLoadHandle(ResourceLoader.HandleBase handle)
        {
            handles.Add(handle);
        }
    }
}