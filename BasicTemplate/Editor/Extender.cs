using System.IO;
using UnityEditor;
using UnityEngine;

namespace Skddkkkk.DevelopKit.BasicTemplate.Editor
{
    [InitializeOnLoad]
    public class Extender : Editor
    {
        private const string UNITASK_NAME = "com.cysharp.unitask";
        private const string UNITASK_URL = "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask";
        
        static BasicTemplateToolbarExtender()
        {
            bool checkUnitaskInstalled = CheckPackageInstalled(UNITASK_NAME);
            if (!checkUnitaskInstalled)
            {
                AddPackage(UNITASK_NAME, UNITASK_URL);
            }
        }

        private static void AddPackage(string name, string url)
        {
            string manifestPath = Path.Combine(Application.dataPath.Replace("Assets", string.Empty), "Packages/manifest.json");
            if (!File.Exists(manifestPath))
            {
                Debug.LogError($"manifest.json not found at '{manifestPath}'");
                return;
            }
            
            string manifestText = File.ReadAllText(manifestPath);
            if (!manifestText.Contains(UNITASK_NAME))
            {
                Debug.Log($"{UNITASK_NAME} not found in manifest.json");
                var modifiedText = manifestText.Insert(manifestText.IndexOf("dependencies") + 17, $"\t\"{UNITASK_NAME}\": \"{UNITASK_URL}\",\n");
                File.WriteAllText(manifestPath, modifiedText);
                Debug.Log($"Added {UNITASK_NAME} to manifest.json");
            }
            UnityEditor.PackageManager.Client.Resolve();
        }

        private static bool CheckPackageInstalled(string packageName)
        {
            string manifestPath = Path.Combine(Application.dataPath.Replace("Assets", string.Empty), "Packages/manifest.json");
            string manifestText = File.ReadAllText(manifestPath);
            return manifestText.Contains(packageName);
        }
    }
}