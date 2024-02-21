using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;
using Kaede2.ScriptableObjects;

namespace Kaede2.Editor
{
    [ScriptedImporter(1, "loopinfo")]
    public class LoopInfoImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (ctx.assetPath == null) return;
            var assetFileName = Path.GetFileNameWithoutExtension(ctx.assetPath);
            var text = File.ReadAllText(ctx.assetPath);
            var data = ScriptableObject.CreateInstance<AudioLoopInfo>();
            JsonUtility.FromJsonOverwrite(text, data);
            ctx.AddObjectToAsset(ctx.assetPath, data);
        }
    }
}