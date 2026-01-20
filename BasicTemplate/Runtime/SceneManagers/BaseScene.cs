#if UNITASK_INSTALLED
using Cysharp.Threading.Tasks;
#endif
using Skddkkkk.DevelopKit.BasicTemplate.Runtime;

namespace Skddkkkk.Developkit.BasicTemplate.Runtime
{
    public class BaseScene : MonoSingleton<BaseScene>
    {
        protected override void Awake()
        {
            base.Awake();
            if (SceneLoadManager.Instance != null)
            {
                SceneLoadManager.Instance.RegisterScene(this);
            }
        }
#if UNITASK_INSTALLED
        public virtual UniTask OnInit()
        {
            return UniTask.CompletedTask;
        }
#endif
        public virtual void OnAfterInit()
        {
        }
    }
}