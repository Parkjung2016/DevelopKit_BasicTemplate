#if UNITASK_INSTALLED
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public abstract class SceneTransitionBase : MonoBehaviour, ISceneTransition
    {
        protected virtual void OnEnable()
        {
            if (Application.isPlaying)
                SceneLoadManager.Instance.SetTransition(this);
        }

        protected virtual void OnDisable()
        {
            if (SceneLoadManager.HasInstance)
                SceneLoadManager.Instance.ClearTransition(this);
        }

#if UNITASK_INSTALLED
        public abstract UniTask OnFadeOut();
        public abstract UniTask OnFadeIn();
#endif
    }
}
