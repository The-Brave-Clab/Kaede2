using System;
using System.IO;
using System.Linq;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Kaede2.Editor.Importers
{
    [ScriptedImporter(1, "masterdata")]
    public class MasterDataImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (ctx.assetPath == null) return;
            var assetFileName = Path.GetFileNameWithoutExtension(ctx.assetPath);

            Type masterDataType = FindType(assetFileName);
            if (masterDataType == null) return;

            var text = File.ReadAllText(ctx.assetPath);
            var data = ScriptableObject.CreateInstance(masterDataType);
            JsonUtility.FromJsonOverwrite(text, data);
            ctx.AddObjectToAsset(ctx.assetPath, data);
        }

        private static Type FindType(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => type.Name == name);
        }
    }
}
