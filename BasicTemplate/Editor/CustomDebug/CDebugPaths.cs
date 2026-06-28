using System.IO;
using UnityEditor;
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Editors
{
    internal static class CDebugPaths
    {
        private const string TagsFolderName = "Tags";
        private const string FallbackFolder = "Assets/BasicTemplate/Runtime/CustomDebug";

        private static string CustomDebugFolder => ResolveCustomDebugFolder();

        public static string TagsFolder => $"{CustomDebugFolder}/{TagsFolderName}";

        public static string DefinitionsAssetPath => $"{TagsFolder}/CDebugTagDefinitions.asset";

        public static string EnumFilePath => $"{TagsFolder}/CDebugTag.cs";

        public static string EditorGeneratedFolder =>
            CustomDebugFolder.Replace("/Runtime/CustomDebug", "/Editor/CustomDebug/Generated");

        public static string ConsoleNavigationGeneratedFilePath =>
            $"{EditorGeneratedFolder}/{CDebugConsoleNavigationGenerator.GeneratedFileName}";

        private static string ResolveCustomDebugFolder()
        {
            return FallbackFolder;
        }
    }
}
