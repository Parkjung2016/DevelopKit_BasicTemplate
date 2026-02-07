using System;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITASK_INSTALLED
using Cysharp.Threading.Tasks;
#endif

namespace Skddkkkk.DevelopKit.BasicTemplate.Runtime
{
    public class SceneLoadManager : PersistentMonoSingleton<SceneLoadManager>
    {
        [SerializeField] private SceneTransitionBase transition;

        private BaseScene curScene;

        public void RegisterScene(BaseScene scene)
        {
            curScene = scene;
        }

        public T GetCurScene<T>() where T : BaseScene
        {
            return GetCurScene() as T;
        }

        public BaseScene GetCurScene()
        {
            return curScene;
        }

        public void SetTransition(SceneTransitionBase transition)
        {
            if (this.transition != null)
            {
                Destroy(transition.gameObject);
            }

            this.transition = transition;
        }


        public async UniTask LoadScene(Enum sceneType, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            if (transition != null)
                await transition.OnFadeIn();

            await LoadSceneAsync(sceneType.ToString(), loadMode);
            await InitializeScene();

            if (transition != null)
                await transition.OnFadeOut();

            curScene.OnAfterInit();
        }


        private async UniTask LoadSceneAsync(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            var op = SceneManager.LoadSceneAsync(sceneName, loadMode);
            op.allowSceneActivation = false;

            var progress = transition as IProgress<float>;
            while (op.progress < 0.9f)
            {
                progress?.Report(op.progress / 0.9f);

                await UniTask.Yield();
            }

            progress?.Report(1f);
            op.allowSceneActivation = true;

            await UniTask.WaitUntil(() => op.isDone);
        }

        private async UniTask InitializeScene()
        {
            if (curScene == null)
            {
                CDebug.LogError("Current Scene is null. Make sure the scene has a BaseScene derived object.");
                return;
            }

#if UNITASK_INSTALLED
            await curScene.OnInit();
#endif
        }
    }
}