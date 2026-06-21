using System.IO;
using PJDev.DevelopKit.BasicTemplate.Runtime;
using UnityEditor;
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Editors
{
    internal static class CDebugTagDefinitionsProvider
    {
        public static CDebugTagDefinitions GetOrCreate()
        {
            var asset = AssetDatabase.LoadAssetAtPath<CDebugTagDefinitions>(CDebugPaths.DefinitionsAssetPath);
            if (asset != null)
                return asset;

            var folder = CDebugPaths.TagsFolder;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            asset = ScriptableObject.CreateInstance<CDebugTagDefinitions>();
            AssetDatabase.CreateAsset(asset, CDebugPaths.DefinitionsAssetPath);
            AssetDatabase.SaveAssets();
            CDebugTagEnumGenerator.Generate(asset);
            return asset;
        }
    }
}
