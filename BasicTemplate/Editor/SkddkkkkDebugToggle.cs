using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using System.Linq;
using UnityEngine;

namespace Skddkkkk.DevelopKit.BasicTemplate.Editor
{
    [InitializeOnLoad]
    public static class SkddkkkkDebugToggle
    {
        private const string SYMBOL = "ENABLE_LOG";
        private const string MENU_PATH = "Skddkkkk/Toggle Debug Logs";

        private static bool prevToggle;

        static SkddkkkkDebugToggle()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(OnBuildPlayer);
            UpdateMenuCheck(); // 에디터 로드 시 체크 상태 갱신
        }

        private static void OnBuildPlayer(BuildPlayerOptions options)
        {
            prevToggle = IsSymbolEnabled();
            bool enableLogs = EditorUtility.DisplayDialog(
                "Enable Debug Logs",
                "빌드에서 SkddkkkkDebug를 활성화하시겠습니까?",
                "Yes",
                "No"
            );

            if (enableLogs != prevToggle)
                SetSymbolEnabled(enableLogs);

            AssetDatabase.Refresh();

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log("빌드 성공");
            else
                Debug.LogError("빌드 실패");
            if (enableLogs != prevToggle)
                SetSymbolEnabled(prevToggle);
        }

        [MenuItem(MENU_PATH)]
        private static void ToggleLogs()
        {
            bool isEnabled = IsSymbolEnabled();
            SetSymbolEnabled(!isEnabled);
            Debug.Log($"SkddkkkkDebug: Logs {(isEnabled ? "disabled" : "enabled")}");
            AssetDatabase.Refresh();
        }

        [MenuItem(MENU_PATH, true)]
        private static bool ToggleLogsValidate()
        {
            UpdateMenuCheck();
            return true;
        }

        private static bool IsSymbolEnabled()
        {
            var targetGroup = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            PlayerSettings.GetScriptingDefineSymbols(targetGroup, out string[] symbols);
            return symbols.Contains(SYMBOL);
        }


        private static void SetSymbolEnabled(bool enabled)
        {
            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            PlayerSettings.GetScriptingDefineSymbols(namedTarget, out string[] symbols);

            var symbolList = symbols.ToList();

            if (enabled && !symbolList.Contains(SYMBOL))
                symbolList.Add(SYMBOL);
            else if (!enabled && symbolList.Contains(SYMBOL))
                symbolList.Remove(SYMBOL);

            PlayerSettings.SetScriptingDefineSymbols(namedTarget, symbolList.ToArray());

            UpdateMenuCheck(); // 메뉴 체크 갱신
        }

        private static void UpdateMenuCheck()
        {
            Menu.SetChecked(MENU_PATH, IsSymbolEnabled());
        }
    }
}