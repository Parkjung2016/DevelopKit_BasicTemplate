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
        public BaseScene CurScene { get; private set; }

        [SerializeField] private SceneTransitionBase transition;

        public void RegisterScene(BaseScene scene)
        {
            CurScene = scene;
        }

        public void SetTransition(SceneTransitionBase transition)
        {
            if (this.transition != null)
            {
                Destroy(transition.Go);
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

            CurScene.OnAfterInit();
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
			if(CurScene== null)
            {
                SkddkkkkDebug.LogError("Current Scene is null. Make sure the scene has a BaseScene derived object.");
                return;
            }
			
#if UNITASK_INSTALLED
            await CurScene.OnInit();
#endif
        }
    }
}