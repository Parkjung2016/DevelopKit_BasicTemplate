using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Skddkkkk.DevelopKit.BasicTemplate.Runtime;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Skddkkkk.DevelopKit.BasicTemplate.Editor
{
    public class SkddkkkkDebugRedirect : AssetPostprocessor
    {
        [OnOpenAsset(0)]
        static bool OnOpenAsset(int instance, int line)
        {
            string name = EditorUtility.InstanceIDToObject(instance).name;
            if (name != nameof(SkddkkkkDebug))
                return false;

            string stackTrace = GetStackTrace();
            if (string.IsNullOrEmpty(stackTrace))
                return false;

            MatchCollection matches = Regex.Matches(stackTrace, @"\(at (.+)\)", RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                string pathLine = match.Groups[1].Value.Trim();
                if (!pathLine.Contains(nameof(SkddkkkkDebug))) 
                {
                    int splitIndex = pathLine.LastIndexOf(":");
                    if (splitIndex > 0)
                    {
                        string path = pathLine.Substring(0, splitIndex);
                        line = Convert.ToInt32(pathLine.Substring(splitIndex + 1));

                        string fullPath = null;

                        if (path.StartsWith("Assets")) 
                        {
                            fullPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets")) + path;
                        }
                        else if (path.StartsWith("Packages")) 
                        {
                            fullPath = path.Replace("Packages",
                                Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets")) + "Library/PackageCache");
                        }
                        else if (path.StartsWith("./Library")) 
                        {
                            fullPath = path.Replace("./",
                                Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets")));
                        }

                        if (!string.IsNullOrEmpty(fullPath))
                        {
                            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(fullPath, line);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        static string GetStackTrace()
        {
            var assemblyUnityEditor = Assembly.GetAssembly(typeof(EditorWindow));
            if (assemblyUnityEditor == null)
                return null;

            var typeConsoleWindow = assemblyUnityEditor.GetType("UnityEditor.ConsoleWindow");
            if (typeConsoleWindow == null)
                return null;

            var fieldConsoleWindow =
                typeConsoleWindow.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            if (fieldConsoleWindow == null)
                return null;

            var instanceConsoleWindow = fieldConsoleWindow.GetValue(null);
            if (instanceConsoleWindow == null)
                return null;

            if (EditorWindow.focusedWindow == (EditorWindow)instanceConsoleWindow)
            {
                var fieldActiveText =
                    typeConsoleWindow.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fieldActiveText == null)
                    return null;

                return fieldActiveText.GetValue(instanceConsoleWindow)?.ToString();
            }

            return null;
        }
    }
}
