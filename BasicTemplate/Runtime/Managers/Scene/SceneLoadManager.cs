using System;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITASK_INSTALLED
using Cysharp.Threading.Tasks;
#endif

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>화면 전환 효과와 BaseScene 초기화 순서를 관리하는 영구 Scene 로더입니다.</summary>
    public sealed class SceneLoadManager : PersistentMonoSingleton<SceneLoadManager>
    {
        [SerializeField] private InterfaceReference<ISceneTransition> transition;

        /// <summary>현재 등록된 BaseScene입니다. Scene에 BaseScene이 없으면 null입니다.</summary>
        public BaseScene CurrentScene { get; private set; }
        /// <summary>Scene 전환 작업이 진행 중인지 나타냅니다.</summary>
        public bool IsLoading { get; private set; }

        /// <summary>현재 Scene을 원하는 BaseScene 파생 타입으로 반환합니다.</summary>
        public T GetCurrentScene<T>() where T : BaseScene => CurrentScene as T;

        public T GetCurScene<T>() where T : BaseScene => GetCurrentScene<T>();

        public BaseScene GetCurScene() => CurrentScene;

        /// <summary>다음 Scene 이동에 사용할 Fade 전환 구현을 지정합니다.</summary>
        public void SetTransition(ISceneTransition value)
        {
            transition ??= new InterfaceReference<ISceneTransition>();
            transition.Value = value;
        }

        internal void ClearTransition(ISceneTransition value)
        {
            if (transition?.Value == value)
                transition.Value = null;
        }

        internal void RegisterScene(BaseScene scene) => CurrentScene = scene;

        internal void UnregisterScene(BaseScene scene)
        {
            if (CurrentScene == scene)
                CurrentScene = null;
        }

#if UNITASK_INSTALLED
        /// <summary>Enum 이름과 같은 Scene을 비동기로 불러옵니다.</summary>
        public UniTask LoadScene(Enum scene, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            return LoadScene(scene.ToString(), mode);
        }

        /// <summary>FadeOut, Scene Load, BaseScene 초기화, FadeIn 순서로 Scene을 전환합니다.</summary>
        public async UniTask LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                throw new ArgumentException("Scene name cannot be empty.", nameof(sceneName));
            if (IsLoading)
                throw new InvalidOperationException("A scene is already loading.");

            IsLoading = true;
            ISceneTransition activeTransition = transition?.Value;
            try
            {
                if (activeTransition != null)
                    await activeTransition.OnFadeOut();

                CurrentScene = null;
                await LoadSceneAsync(sceneName, mode, activeTransition as IProgress<float>);

                BaseScene loadedScene = CurrentScene;
                if (loadedScene == null)
                {
                    CDebug.LogError(
                        $"Scene '{sceneName}' has no BaseScene component. Scene initialization was skipped.");
                }
                else
                {
                    await loadedScene.OnInit();
                }

                if (activeTransition != null)
                    await activeTransition.OnFadeIn();

                loadedScene?.OnAfterInit();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private static async UniTask LoadSceneAsync(
            string sceneName,
            LoadSceneMode mode,
            IProgress<float> progress)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);
            if (operation == null)
                throw new InvalidOperationException($"Could not start loading scene '{sceneName}'.");

            operation.allowSceneActivation = false;
            while (operation.progress < 0.9f)
            {
                progress?.Report(operation.progress / 0.9f);
                await UniTask.Yield();
            }

            progress?.Report(1f);
            operation.allowSceneActivation = true;
            await UniTask.WaitUntil(() => operation.isDone);
        }
#endif
    }
}
