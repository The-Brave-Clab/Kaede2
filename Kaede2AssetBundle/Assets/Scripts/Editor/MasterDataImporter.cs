using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;
using Kaede2.Assets.ScriptableObjects;

namespace Kaede2.Assets.Editor
{
    [ScriptedImporter(1, "masterdata")]
    public class MasterDataImporter : ScriptedImporter
    {
        static List<Type> masterDataTypes = null;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (masterDataTypes == null)
            {
                masterDataTypes = new List<Type>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(BaseMasterData)))
                        {
                            masterDataTypes.Add(type);
                        }
                    }
                }
            }
            

            if (ctx.assetPath == null) return;
            var assetFileName = Path.GetFileNameWithoutExtension(ctx.assetPath);
            if (masterDataTypes.Exists(t => t.Name == assetFileName))
            {
                var text = File.ReadAllText(ctx.assetPath);
                var type = masterDataTypes.Find(t => t.Name == assetFileName);
                var data = ScriptableObject.CreateInstance(type);
                JsonUtility.FromJsonOverwrite(text, data);
                ctx.AddObjectToAsset(ctx.assetPath, data);
            }
        }
    }
}
