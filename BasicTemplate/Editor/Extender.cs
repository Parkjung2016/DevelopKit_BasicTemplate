using PJDev.DevelopKit.Editors;
using UnityEditor;

namespace PJDev.DevelopKit.BasicTemplate.Editors
{
    [InitializeOnLoad]
    public class Extender : Editor
    {
        private const string UNITASK_NAME = "com.cysharp.unitask";

        private const string UNITASK_URL =
            "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask";

        static Extender()
        {
            bool checkUniTaskInstalled = DevelopKitEditorUtility.CheckPackageInstalled(UNITASK_NAME);
            if (!checkUniTaskInstalled)
            {
                DevelopKitEditorUtility.AddPackage(UNITASK_NAME, UNITASK_URL);
            }
        }
    }
}