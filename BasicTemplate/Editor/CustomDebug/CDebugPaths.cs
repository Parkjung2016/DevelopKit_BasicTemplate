using System.IO;
using UnityEditor;
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Editors
{
    internal static class CDebugPaths
    {
        private const string TagsFolderName = "Tags";
        private const string FallbackFolder = "Assets/BasicTemplate/BasicTemplate/Runtime/CustomDebug";

        public static string CustomDebugFolder => ResolveCustomDebugFolder();

        public static string TagsFolder => $"{CustomDebugFolder}/{TagsFolderName}";

        public static string DefinitionsAssetPath => $"{TagsFolder}/CDebugTagDefinitions.asset";

        public static string EnumFilePath => $"{TagsFolder}/CDebugTag.cs";

        private static string ResolveCustomDebugFolder()
        {
            var guids = AssetDatabase.FindAssets("CDebug t:Script");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("/CustomDebug/CDebug.cs"))
                    return Path.GetDirectoryName(path)?.Replace('\\', '/');
            }

            return FallbackFolder;
        }
    }
}
