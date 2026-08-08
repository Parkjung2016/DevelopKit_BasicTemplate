#if UNITASK_INSTALLED
using Cysharp.Threading.Tasks;
#endif

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public class BaseScene : MonoSingleton<BaseScene>
    {
        protected override void Awake()
        {
            base.Awake();
            if (!UnityEngine.Application.isPlaying || instance != this)
                return;

            SceneLoadManager.Instance.RegisterScene(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (SceneLoadManager.HasInstance)
                SceneLoadManager.Instance.UnregisterScene(this);
        }

#if UNITASK_INSTALLED
        /// <summary>씬 활성화 직후 필요한 비동기 초기화를 수행합니다.</summary>
        public virtual UniTask OnInit() => UniTask.CompletedTask;
#endif

        /// <summary>씬 초기화와 화면 전환이 모두 끝난 뒤 호출됩니다.</summary>
        public virtual void OnAfterInit() { }
    }
}
