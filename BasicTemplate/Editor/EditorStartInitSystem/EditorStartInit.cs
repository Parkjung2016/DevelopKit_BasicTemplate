using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Skddkkkk.DevelopKit.BasicTemplate.Editors
{
    [InitializeOnLoad]
    public class EditorStartInit
    {
        private static bool UseSetupScene
        {
            get => EditorPrefs.GetBool("UseSetupScene", false);
            set => EditorPrefs.SetBool("UseSetupScene", value);
        }

        static EditorStartInit()
        {
            ApplyPlayModeStartScene();
        }

        [MenuItem("SetupScene/Use Setup Scene")]
        private static void ToggleUseSetupScene()
        {
            UseSetupScene = !UseSetupScene;

            ApplyPlayModeStartScene();
        }

        [MenuItem("SetupScene/Use Setup Scene", true)]
        private static bool ToggleUseSetupSceneValidate()
        {
            Menu.SetChecked("SetupScene/Use Setup Scene", UseSetupScene);
            return true;
        }

        private static void ApplyPlayModeStartScene()
        {
            if (UseSetupScene)
            {
                var pathOfFirstScene = EditorBuildSettings.scenes[0].path;
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(pathOfFirstScene);
                EditorSceneManager.playModeStartScene = sceneAsset;
                Debug.Log($"▶ Setup 씬에서 시작: {pathOfFirstScene}");
            }
            else
            {
                EditorSceneManager.playModeStartScene = null;
                Debug.Log("▶ 현재 씬에서 시작");
            }
        }
    }
}