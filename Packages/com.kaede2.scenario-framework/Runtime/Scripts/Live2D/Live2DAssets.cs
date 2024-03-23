using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Live2D
{
    public class Live2DAssets : ScriptableObject
    {
        public string modelName;
        public TextAsset mocFile;
        public Texture2D[] textures;
        public List<MotionFile> motionFiles;
        public TextAsset poseFile;

        [Serializable]
        public class MotionFile
        {
            public string name;
            public List<TextAsset> files;
        }

        public MotionFile GetMotionFile(string motionName)
        {
            return motionFiles.Find(motionFile => motionFile.name == motionName);
        }
    }
}